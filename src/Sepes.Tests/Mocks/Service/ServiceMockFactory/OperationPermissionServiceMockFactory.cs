using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public static class OperationPermissionServiceMockFactory
    {      

        public static IOperationPermissionService Create(IUserService userService = null) {                    

            return new OperationPermissionService(userService == null ? UserServiceMockFactory.GetUserServiceMockForAdmin(1).Object : userService);
        }       
    }
}
