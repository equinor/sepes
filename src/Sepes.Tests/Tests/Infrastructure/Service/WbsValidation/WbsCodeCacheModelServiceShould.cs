//using System.Threading.Tasks;
//using Xunit;

//namespace Sepes.Tests.Services.Infrastructure
//{
//    public class WbsCodeCacheModelServiceShould : WbsValidationServiceTestBase
//    { 
//        [Theory]
//        [InlineData("someWbs", 60)]
//        [InlineData("somewbs", 60)]
//        [InlineData("SOMEWBS", 60)]
//        public async Task ReturnItemForSingleValidWbs(string wbsCode, int expiresInSeconds)
//        {
//            var service = await GetCacheService(wbsCode, true, expiresInSeconds);

//            var cachedItem = await service.Get(wbsCode);
//            Assert.NotNull(cachedItem);
//            Assert.True(cachedItem.Valid);
//        }

//        [Fact]
//        public async Task ReturnNullIfCacheIsEmpty()
//        {
//            var service = await GetCacheService();

//            var cachedItem = await service.Get("somewbs");
//            Assert.Null(cachedItem);         
//        }

//        [Fact]
//        public async Task ReturnNullIfNoRelevantMatches()
//        {
//            var service = await GetCacheService("someOtherWbs", true, 10);

//            var cachedItem = await service.Get("somewbs");
//            Assert.Null(cachedItem);          
//        }

//        [Theory]
//        [InlineData("someWbs", 60)]
//        [InlineData("somewbs", 60)]
//        [InlineData("SOMEWBS", 60)]
//        public async Task KeepNonExpiredCodesOnClean(string wbsCode, int expiresInSeconds)
//        {
//            var service = await GetCacheService(wbsCode, true, expiresInSeconds);

//            await service.Clean();
//            var cachedItem = await service.Get(wbsCode);

//            Assert.NotNull(cachedItem);
//        }

//        [Theory]
//        [InlineData("someWbs", -1)]
//        [InlineData("somewbs", -1)]
//        [InlineData("SOMEWBS", -1)]
//        public async Task DeleteExpiredCodesOnClean(string wbsCode, int expiresInSeconds)
//        {
//            var service = await GetCacheService(wbsCode, true, expiresInSeconds);

//            await service.Clean();
//            var cachedItem = await service.Get(wbsCode);

//            Assert.Null(cachedItem);
//        }
//    }
//}
