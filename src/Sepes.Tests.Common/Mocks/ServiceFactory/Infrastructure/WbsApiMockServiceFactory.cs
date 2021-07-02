using Moq;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Threading;

namespace Sepes.Tests.Common.ServiceMockFactories.Infrastructure
{
    public static class WbsApiMockServiceFactory
    {
        public static IWbsApiService GetService(bool isValidReturnsTrue, bool isValidThrows)
        {
            var wbsValidationServiceMock = new Mock<IWbsApiService>();

            if (isValidThrows)
            {
                wbsValidationServiceMock.Setup(s =>
                        s.Exists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());
            }
            else
            {
                wbsValidationServiceMock.Setup(s =>
                        s.Exists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isValidReturnsTrue);
            }           

            return wbsValidationServiceMock.Object;
        }
    }
}
