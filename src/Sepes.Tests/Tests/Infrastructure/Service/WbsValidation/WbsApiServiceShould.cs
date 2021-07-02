using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Sepes.Tests.Services.Infrastructure
{
    public class WbsApiServiceShould : WbsValidationServiceTestBase
    {
        [Theory]
        [InlineData("somewbs")]
        [InlineData("someWbs")]
        [InlineData("SOMEWBS")]
        public async Task ReturnTrueForSingleValidWbs(string wbs)
        {
            var service = GetApiService(wbsCodesInApiResponse: new string[] { wbs });

            var result = await service.Exists(wbs);

            Assert.True(result);
        }

        [Fact]             
        public async Task ReturnFalseIfNoMatches()
        {
            var service = GetApiService();

            var result = await service.Exists("somewbs");

            Assert.False(result);
        }

        [Theory]
        [InlineData("someWbs", "anotherWbs")]
        [InlineData("someWbs", "anotherWbs", "aThirdWbs")]
        public async Task ReturnFalseIfMultipleMatches(params string[] wbsCodesInResponse)
        {
            var service = GetApiService(wbsCodesInApiResponse: wbsCodesInResponse);

            var result = await service.Exists(wbsCodesInResponse[0]);

            Assert.False(result);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest, "someWbs")]
        [InlineData(HttpStatusCode.Unauthorized, "someWbs")]
        [InlineData(HttpStatusCode.NotFound, "someWbs")]
        [InlineData(HttpStatusCode.Forbidden, "someWbs")]
        public async Task ReturnFalseIfRequestFails(HttpStatusCode httpStatusCode, params string[] wbsCodesInResponse)
        {
            var service = GetApiService(httpStatusCode, wbsCodesInApiResponse: wbsCodesInResponse);

            var result = await service.Exists(wbsCodesInResponse[0]);

            Assert.False(result);
        }       
    }
}
