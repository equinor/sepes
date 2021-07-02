using Moq;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Provisioning;
using Sepes.Azure.Service.Interface;
using Sepes.Tests.Common.ServiceMockFactories.Azure;
using System.Threading;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class AzureVirtualNetworkMockServiceFactory
    {
        public static Mock<IAzureVirtualNetworkService> CreateBasicForCreate() {

            var mock = new Mock<IAzureVirtualNetworkService>();          
            mock.Setup(us => us.EnsureCreated(It.IsAny<ResourceProvisioningParameters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ResourceProvisioningParameters ipt, CancellationToken cancellation) => ProvisioningResultFactory.Create(ipt, AzureResourceType.VirtualNetwork));
            mock.Setup(us => us.EnsureSandboxSubnetHasServiceEndpointForStorage(It.IsAny<string>(), It.IsAny<string>()));
              

            return mock;

        }
    }
}
