using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public static class StudyModelServiceMockFactory
    {
        public static IStudyListModelService StudyListModelService(ServiceProvider serviceProvider)
        {                
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);
            var studyPermissionService = StudyPermissionServiceMockFactory.Create(serviceProvider);
            var dapperQueryService = serviceProvider.GetService<IDapperQueryService>();
            return new StudyListModelService(dapperQueryService, userService.Object, studyPermissionService);
        }

        public static IStudyEfModelService StudyEfModelService(ServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var db = serviceProvider.GetService<SepesDbContext>();
            var logger = serviceProvider.GetService<ILogger<StudyEfModelService>>();

            var studyEfModelOperationsService = StudyEfModelOperationsServiceMockFactory.Create(serviceProvider);
            var studyPermissionService = StudyPermissionServiceMockFactory.Create(serviceProvider);

            return new StudyEfModelService(config, db, logger, studyEfModelOperationsService, studyPermissionService);
        } 
    }
}
