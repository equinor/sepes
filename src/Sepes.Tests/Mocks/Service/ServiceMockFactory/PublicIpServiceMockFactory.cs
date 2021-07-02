using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Common.Mocks.Service;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public static class PublicIpServiceMockFactory
    {
        public static IPublicIpService CreateSucceedingService(ServiceProvider serviceProvider, string ipAddress, int succeedAfterNoTries = 1)
        {
            var configuration = serviceProvider.GetService<IConfiguration>();
            var logger = serviceProvider.GetService<ILogger<PublicIpService>>();
            var thirdPartyService = CreateSucceedingAfterCertainTriesThirdPartyService(ipAddress, succeedAfterNoTries);
            return new PublicIpService(configuration, logger, thirdPartyService);
        }

        public static IPublicIpService CreateFailingService(ServiceProvider serviceProvider, Exception ex)
        {
            var configuration = serviceProvider.GetService<IConfiguration>();
            var logger = serviceProvider.GetService<ILogger<PublicIpService>>();
            var thirdPartyService = CreateThrowingThirdPartyService(ex);
            return new PublicIpService(configuration, logger, thirdPartyService);
        }

        public static IPublicIpFromThirdPartyService CreateSucceedingAfterCertainTriesThirdPartyService(string ipAddress = "10.1.1.12", int succeedAfterNoTries = 1)
        {
            var mockService = new PublicIpFromThirdPartyServiceMock(succeedAfterNoTries, ipAddress);        
            return mockService;
        }

        public static IPublicIpFromThirdPartyService CreateThrowingThirdPartyService(Exception ex)
        {           
            var mockService = new Mock<IPublicIpFromThirdPartyService>();
            mockService.Setup(x => x.GetIp(It.IsAny<string>(), default)).ThrowsAsync(ex);
            return mockService.Object;
        }
    }
}
