using Moq;
using Sepes.Provisioning.Service.Interface;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class ProvisioningLogServiceMock
    {
        public static Mock<IProvisioningLogService> CreateBasic() {

            var mock = new Mock<IProvisioningLogService>();
            return mock;
        }
    }
}
