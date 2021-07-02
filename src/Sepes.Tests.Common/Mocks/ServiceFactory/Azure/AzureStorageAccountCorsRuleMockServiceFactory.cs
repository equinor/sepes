using Moq;
using Sepes.Common.Dto;
using Sepes.Azure.Service.Interface;
using System.Collections.Generic;
using System.Threading;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class AzureStorageAccountCorsRuleMockServiceFactory
    {
        public static Mock<IAzureStorageAccountCorsRuleService> CreateBasicForCreate() {

            var mock = new Mock<IAzureStorageAccountCorsRuleService>();

            mock.Setup(us => us.SetCorsRules(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CorsRule>>(), It.IsAny<CancellationToken>()));               

            return mock;
        }
    }
}
