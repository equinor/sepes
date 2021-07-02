using Microsoft.Extensions.DependencyInjection;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public static class StudyEfModelOperationsServiceMockFactory
    {  
        public static IStudyEfModelOperationsService Create(ServiceProvider serviceProvider, IUserService userService = null) {

            var studyPermissionService = StudyPermissionServiceMockFactory.Create(serviceProvider, userService);

            return new StudyEfModelOperationsService(studyPermissionService);
        }       
    }
}
