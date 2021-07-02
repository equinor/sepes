using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public static class StudyPermissionServiceMockFactory
    {      

        public static IStudyPermissionService Create(ServiceProvider serviceProvider, IUserService userService = null) {

            var mapper = serviceProvider.GetService<IMapper>();            

            return new StudyPermissionService(mapper, userService == null ? UserServiceMockFactory.GetUserServiceMockForAdmin(1).Object : userService);
        }       
    }
}
