using Moq;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Provisioning.Service;
using Sepes.Provisioning.Service.Interface;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class CreateAndUpdateServiceMock
    {
        public static ICreateAndUpdateService CreateBasic() {

            var loggerMock = ProvisioningLogServiceMock.CreateBasic();

            var resourceReadServiceMock = new Mock<ICloudResourceReadService>();
            var resourceUpdateServiceMock = new Mock<ICloudResourceUpdateService>();
            var resourceOperationUpdateServiceMock = new Mock<ICloudResourceOperationUpdateService>();

            var createAndUpdateService = new CreateAndUpdateService(loggerMock.Object, resourceReadServiceMock.Object, resourceUpdateServiceMock.Object, resourceOperationUpdateServiceMock.Object);
                          
            return createAndUpdateService;
        }
    }
}
