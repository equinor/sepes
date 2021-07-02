using System.Threading;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Azure.Service.Interface;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public class StudyParticipantMockFactory
    {
        public static IStudyParticipantSearchService GetStudyParticipantLookupService(ServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<StudyParticipantSearchService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);

            var studyModelService = StudyModelServiceMockFactory.StudyEfModelService(serviceProvider);

            var azureUserLookupService = new Mock<ICombinedUserLookupService>();
            azureUserLookupService.Setup(s => s.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new System.Collections.Generic.Dictionary<string, Sepes.Azure.Dto.AzureUserDto>());

            var provisioningQueueService = new Mock<IProvisioningQueueService>();

            var resourceReadServiceMock = new Mock<ICloudResourceReadService>();

            var cloudResourceOperationCreateService = new Mock<ICloudResourceOperationCreateService>();

            var cloudResourceOperationUpdateService = new Mock<ICloudResourceOperationUpdateService>();       

            return new StudyParticipantSearchService(db, logger, mapper, userService.Object, azureUserLookupService.Object, studyModelService, provisioningQueueService.Object, resourceReadServiceMock.Object, cloudResourceOperationCreateService.Object, cloudResourceOperationUpdateService.Object);
        }
    }
}
