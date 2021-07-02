using Moq;
using Sepes.Common.Dto;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Tests.Common.Constants;
using Sepes.Tests.Tests;
using System.Threading.Tasks;
using Sepes.Tests.Common.Mocks.ServiceFactory;
using Xunit;

namespace Sepes.Tests.Services.DomainServices
{
    public class UserServiceTest : TestBaseWithInMemoryDb
    {
        public UserServiceTest()
            : base()
        {
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public async Task Requesting_User_ShouldReturnUserObjectWithRelevantProperties(bool admin, bool sponsor, bool datasetAdmin, bool employee)
        {
            var userService = await GetUserServiceWithMocks(admin, sponsor, datasetAdmin, employee);

            var userWithoutStudyParticipants = await userService.GetCurrentUserAsync();
            UserDtoAsserts(userWithoutStudyParticipants, admin, sponsor, datasetAdmin, employee);

            var sameUserButWithStudyParticipants = await userService.GetCurrentUserAsync();
            UserDtoAsserts(sameUserButWithStudyParticipants, admin, sponsor, datasetAdmin, employee);

        }

        void UserDtoAsserts(UserDto user, bool admin, bool sponsor, bool datasetAdmin, bool employee)
        {
            Assert.Equal(admin, user.Admin);
            Assert.Equal(sponsor, user.Sponsor);
            Assert.Equal(datasetAdmin, user.DatasetAdmin);
            Assert.Equal(employee, user.Employee);
        }

        async Task<UserService> GetUserServiceWithMocks(bool admin = false, bool sponsor = false, bool datasetAdmin = false, bool employee = false)
        {
            await ClearTestDatabaseAddUser();         

            var userModelServiceMock = new Mock<IUserModelService>();
            userModelServiceMock.Setup(s => s.GetByObjectIdAsync(It.IsAny<string>())).ReturnsAsync(new UserDto() { Id = UserTestConstants.COMMON_CUR_USER_DB_ID, ObjectId = UserTestConstants.COMMON_CUR_USER_OBJECTID, FullName = UserTestConstants.COMMON_CUR_USER_FULL_NAME, UserName = UserTestConstants.COMMON_CUR_USER_UPN });          
          
            var contextUserServiceMock = ContextUserServiceMock.GetService(admin: admin, sponsor: sponsor, datasetAdmin: datasetAdmin, employee: employee);         

            return new UserService(userModelServiceMock.Object, contextUserServiceMock.Object);
        }
    }
}
