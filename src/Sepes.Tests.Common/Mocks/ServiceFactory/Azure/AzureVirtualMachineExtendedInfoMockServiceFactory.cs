using Moq;
using Sepes.Azure.Service.Interface;
using Sepes.Tests.Common.ServiceMockFactories.Azure;
using System.Threading;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class AzureVirtualMachineExtendedInfoMockServiceFactory
    {
        public static Mock<IAzureVirtualMachineExtendedInfoService> CreateBasicForCreate() {

            var mock = new Mock<IAzureVirtualMachineExtendedInfoService>();
         
            mock.Setup(us => us.GetExtendedInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((string resourceGroupName, string resourceName, CancellationToken cancellation) => VmExtendedInfoFactory.Create());

            return mock;

        }
    }
}
