using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sepes.Infrastructure.Handlers;
using Sepes.Infrastructure.Handlers.Interface;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Common.Constants;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.HandlerMockFactory
{
    public static class StudyUpdateHandlerMockFactory   
    {  
        public static IStudyUpdateHandler Create(ServiceProvider serviceProvider, bool wbsValidationSucceeds = true)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(UserTestConstants.COMMON_CUR_USER_DB_ID);
            var logoCreateServiceMock = new Mock<IStudyLogoCreateService>();
            var logoDeleteServiceMock = new Mock<IStudyLogoDeleteService>(); 
            
            var studyWbsValidationService = StudyServiceMockFactory.GetStudyWbsValidationServiceMock(wbsValidationSucceeds);
            var updateStudyWbsHandler = new Mock<IStudyWbsUpdateHandler>();
            var studyEfModelOperationsService = StudyEfModelOperationsServiceMockFactory.Create(serviceProvider);

            return new StudyUpdateHandler(db, userService.Object, logoCreateServiceMock.Object, logoDeleteServiceMock.Object , studyWbsValidationService.Object, updateStudyWbsHandler.Object, studyEfModelOperationsService);
        }     
    }
}
