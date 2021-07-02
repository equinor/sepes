using Moq;
using Sepes.Common.Constants;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System.Threading.Tasks;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services.DomainServices
{
    public partial class OperationPermissionServiceShould : TestBase
    {
        [Theory]  
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ThrowOn_StudyCreate_IfNotAllowed(bool employee, bool datasetAdmin)
        {
            var userService = GetUserServiceMock(employee: employee, datasetAdmin: datasetAdmin);
            await PerformThrowingTest(userService, UserOperation.Study_Create);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ThrowOn_StudyCreateOrUpdate_IfNotAllowed(bool employee, bool datasetAdmin)
        {
            var userService = GetUserServiceMock(employee: employee, datasetAdmin: datasetAdmin);
            await PerformThrowingTestMultipleOperation(userService, UserOperation.Study_Create, UserOperation.Study_Update_Metadata);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public async Task ThrowOn_StudyDelete_IfNotAllowed(bool employee, bool sponsor, bool datasetAdmin)
        {
            var userService = GetUserServiceMock(employee: employee, sponsor: sponsor, datasetAdmin: datasetAdmin);
            await PerformThrowingTest(userService, UserOperation.Study_Delete);
        }

        [Fact]
        
        public async Task ThrowOn_ReadPreApprovedDataset_IfNotAllowed()
        {
            var userService = GetUserServiceMock(employee: false);
            await PerformThrowingTest(userService, UserOperation.PreApprovedDataset_Read);
        }


        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ThrowOn_CreateOrUpdatePreApprovedDataset_IfNotAllowed(bool employee, bool sponsor)
        {
            var userService = GetUserServiceMock(employee: employee, sponsor: sponsor);
            await PerformThrowingTest(userService, UserOperation.PreApprovedDataset_Create_Update_Delete);
        }

        async Task PerformThrowingTest(Mock<IUserService> userService, UserOperation userOperation) {
            var operationPermissionService = OperationPermissionServiceMockFactory.Create(userService.Object);

            await PerformBoolCheck(operationPermissionService, userOperation, false);

            await Assert.ThrowsAsync<ForbiddenException>(() => operationPermissionService.HasAccessToOperationOrThrow(userOperation));
        }

        async Task PerformThrowingTestMultipleOperation(Mock<IUserService> userService, params UserOperation[] userOperations)
        {
            var operationPermissionService = OperationPermissionServiceMockFactory.Create(userService.Object);

            foreach (var curOperation in userOperations)
            {
                await PerformBoolCheck(operationPermissionService, curOperation, false);
            }               

            await Assert.ThrowsAsync<ForbiddenException>(() => operationPermissionService.HasAccessToAnyOperationOrThrow(userOperations));
        }

        Mock<IUserService> GetUserServiceMock(bool employee = false, bool admin = false, bool sponsor = false, bool datasetAdmin = false, int userId = 1)
        {
            if (admin)
            {
               return UserServiceMockFactory.GetUserServiceMockForAdmin(userId);
            }
            else if(sponsor)
            {
                return UserServiceMockFactory.GetUserServiceMockForSponsor(userId);
            }
            else if (datasetAdmin)
            {
                return UserServiceMockFactory.GetUserServiceMockForDatasetAdmin(userId);
            }

            return UserServiceMockFactory.GetUserServiceMockForBasicUser(employee, userId);        
        }
    }
}
