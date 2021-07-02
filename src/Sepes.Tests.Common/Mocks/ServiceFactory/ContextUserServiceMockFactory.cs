using Moq;
using Sepes.Common.Interface;
using Sepes.Tests.Common.Constants;

namespace Sepes.Tests.Common.Mocks.ServiceFactory
{
    public static class ContextUserServiceMock
    {
        public static Mock<IContextUserService> GetService(bool admin = false, bool sponsor = false, bool datasetAdmin = false, bool employee = false)
        {
            var currentUserServiceMock = new Mock<IContextUserService>();           

            currentUserServiceMock.Setup(us => us.GetCurrentUser()).Returns(new Sepes.Common.Dto.UserDto(
                UserTestConstants.COMMON_CUR_USER_OBJECTID,
                UserTestConstants.COMMON_CUR_USER_UPN,
                UserTestConstants.COMMON_CUR_USER_FULL_NAME,
                UserTestConstants.COMMON_CUR_USER_EMAIL,
                admin,
                sponsor, 
                datasetAdmin,
                employee));           

            return currentUserServiceMock;
        }
    }
}
