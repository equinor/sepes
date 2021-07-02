using Moq;
using Sepes.Infrastructure.Service.Interface;

namespace Sepes.Tests.Common.ServiceMockFactories.Infrastructure
{
    public static class DapperQueryServiceMockFactory
    {
        public static Mock<IDapperQueryService> GetBasicService()
        {
            return new Mock<IDapperQueryService>();
        }
    }
}
