using Moq;
using Sepes.Azure.Service.Interface;
using Sepes.Common.Dto.VirtualMachine;
using System.Threading;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class AzureNetworkSecurityGroupRuleMockServiceFactory
    {
        public static Mock<IAzureNetworkSecurityGroupRuleService> CreateWhereRuleSetToReturnsTrue() {

            var mock = new Mock<IAzureNetworkSecurityGroupRuleService>();          
            mock.Setup(us => us.IsRuleSetTo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RuleAction>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            return mock;

        }

        public static Mock<IAzureNetworkSecurityGroupRuleService> CreateWhereRuleSetToReturnsFalse()
        {

            var mock = new Mock<IAzureNetworkSecurityGroupRuleService>();
            mock.Setup(us => us.IsRuleSetTo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RuleAction>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

            return mock;

        }
    }
}
