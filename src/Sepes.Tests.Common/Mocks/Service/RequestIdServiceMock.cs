using Sepes.Common.Interface;

namespace Sepes.Tests.Common.Mocks.Service
{
    public class RequestIdServiceMock : IRequestIdService
    {
        public string GetRequestId() { return "requestId"; }
    }
}
