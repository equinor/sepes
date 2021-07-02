using Microsoft.Identity.Web;
using Moq;

namespace Sepes.Tests.Common.Mocks
{
    public static class TokenAquistionMockFactory
    {
        public static Mock<ITokenAcquisition> CreateDefault()
        {
            var mock = new Mock<ITokenAcquisition>();
            mock.Setup(t => t.GetAccessTokenForAppAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenAcquisitionOptions>())).ReturnsAsync("bogustoken");
            return mock;
        }
    }
}