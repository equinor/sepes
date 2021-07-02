using Moq;
using Sepes.Azure.Service.Interface;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Provisioning;
using Sepes.Tests.Common.ServiceMockFactories.Azure;
using System.Threading;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class AzureResourceGroupMockServiceFactory
    {
        public static Mock<IAzureResourceGroupService> CreateBasicForCreate() {

            var mock = new Mock<IAzureResourceGroupService>();
         
            mock.Setup(us => us.EnsureCreated(It.IsAny<ResourceProvisioningParameters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ResourceProvisioningParameters ipt, CancellationToken cancellation) => ProvisioningResultFactory.Create(ipt, AzureResourceType.ResourceGroup));

            return mock;

        }
    }
}
