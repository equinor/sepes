using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System;
using System.Threading.Tasks;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services
{
    public class PublicIpServiceTests : TestBase
    {
        [Theory]
        [InlineData("192.168.1.1")]
        public async Task GettingIp_ShouldHandleRetries(string expectedIp)
        {
            var publicIpThirdPartyService = PublicIpServiceMockFactory.CreateSucceedingService(_serviceProvider, expectedIp);
            var publicIpFromService = await publicIpThirdPartyService.GetIp();
            Assert.Equal(expectedIp, publicIpFromService);

            publicIpThirdPartyService = PublicIpServiceMockFactory.CreateSucceedingService(_serviceProvider, expectedIp, 2);
            publicIpFromService = await publicIpThirdPartyService.GetIp();
            Assert.Equal(expectedIp, publicIpFromService);

            publicIpThirdPartyService = PublicIpServiceMockFactory.CreateSucceedingService(_serviceProvider, expectedIp, 3);
            publicIpFromService = await publicIpThirdPartyService.GetIp();
            Assert.Equal(expectedIp, publicIpFromService);         
        }

        [Fact]
        public async Task GettingIp_WhenDownstreamSystemFails_ShouldThrow()
        {
            var exceptionToThrow = new Exception();
            var publicIpThirdPartyService = PublicIpServiceMockFactory.CreateFailingService(_serviceProvider, exceptionToThrow);
            await Assert.ThrowsAsync<Exception>(() => publicIpThirdPartyService.GetIp());

            publicIpThirdPartyService = PublicIpServiceMockFactory.CreateSucceedingService(_serviceProvider, "does not matter", 4);
            await Assert.ThrowsAsync<Exception>(() => publicIpThirdPartyService.GetIp());
        }

    }
}
