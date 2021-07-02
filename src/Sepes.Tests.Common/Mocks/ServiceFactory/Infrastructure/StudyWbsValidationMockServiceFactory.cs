using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Threading;

namespace Sepes.Tests.Common.ServiceMockFactories.Infrastructure
{
    public static class StudyWbsValidationMockServiceFactory
    {
        public static IStudyWbsValidationService GetService(IServiceProvider serviceProvider, bool wbsExists, bool willFail)
        {
            var wbsValidationServiceMock = new Mock<IWbsValidationService>();

            if (willFail)
            {
                wbsValidationServiceMock.Setup(s =>
                        s.IsValid(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());
            }
            else
            {
                wbsValidationServiceMock.Setup(s =>
                        s.IsValid(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(wbsExists);
            }

            var logger = serviceProvider.GetService<ILogger<StudyWbsValidationService>>();

            Mock<IDapperQueryService> dapperQueryService = DapperQueryServiceMockFactory.GetBasicService();

            return new StudyWbsValidationService(logger, wbsValidationServiceMock.Object, dapperQueryService.Object);
        }
    }
}
