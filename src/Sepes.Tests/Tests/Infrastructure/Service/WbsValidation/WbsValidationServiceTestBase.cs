using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Test.Common.ServiceMockFactories;
using Sepes.Tests.Common.Mocks;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sepes.Tests.Mocks.ServiceMockFactory;

namespace Sepes.Tests.Services.Infrastructure
{
    public class WbsValidationServiceTestBase : TestBaseWithInMemoryDb
    {
        protected IWbsApiService GetApiService(HttpStatusCode httpStatusCode = HttpStatusCode.OK, params string[] wbsCodesInApiResponse)
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(CreateWbsApiResponseString(wbsCodesInApiResponse)),
                })
                .Verifiable();

            return new WbsApiService(_serviceProvider.GetService<IConfiguration>(),
                _serviceProvider.GetService<ILogger<WbsApiService>>(),
                TokenAquistionMockFactory.CreateDefault().Object,
                new HttpClient(httpMessageHandlerMock.Object));
        }

        string CreateWbsApiResponseString(string[] wbsCodesInApiResponse = null)
        {
            var sbResponse = new StringBuilder();
            sbResponse.Append("[");

            foreach (var curWbsCode in wbsCodesInApiResponse)
            {
                if (sbResponse.Length > 1)
                {
                    sbResponse.Append(",");
                }
                sbResponse.Append("{\"code\":\"" + curWbsCode + "\"}");
            }

            sbResponse.Append("]");

            return sbResponse.ToString();
        }

        protected async Task<IWbsCodeCacheModelService> GetCacheService(string wbsCode, bool valid, int expiresInSeconds)
        {
            return await GetCacheService(
                new List<WbsCodeCache>() {
                    new WbsCodeCache(wbsCode.ToLowerInvariant(), valid, DateTime.UtcNow.AddSeconds(expiresInSeconds))
                });
        }

        protected async Task<IWbsCodeCacheModelService> GetCacheService(params WbsCodeCache[] wbsCodesInCache)
        {
            var wbsCodesInCacheList = new List<WbsCodeCache>();

            if (wbsCodesInCache.Length > 0)
            {
                wbsCodesInCacheList.AddRange(wbsCodesInCache);
            }

            return await GetCacheService(wbsCodesInCacheList);
        }

        async Task<IWbsCodeCacheModelService> GetCacheService(List<WbsCodeCache> wbsCodesInCache)
        {
            var db = await ClearTestDatabase();

            var wbsCodesLookup = wbsCodesInCache.ToDictionary(w => w.WbsCode.ToLowerInvariant(), w => w);

            var dapperQueryServiceMock = new Mock<IDapperQueryService>();
            dapperQueryServiceMock.Setup(s => s.RunDapperQuerySingleAsync<bool>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((string wbsCode, object parameters) => wbsCodesLookup.ContainsKey(wbsCode));
            dapperQueryServiceMock.Setup(s => s.RunDapperQuerySingleAsync<WbsCodeCache>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((string wbsCode, object parameters) =>
            {
                if (wbsCodesLookup.TryGetValue(wbsCode, out WbsCodeCache itemFromCache))
                {
                    return itemFromCache;
                }
                else
                {
                    return null;
                }
            }
            );

            return new WbsCodeCacheModelService(_serviceProvider.GetService<ILogger<WbsCodeCacheModelService>>(), dapperQueryServiceMock.Object);
        }


        protected IWbsValidationService GetWbsValidationService(bool foundInCache, bool validInCache, bool foundInApi, out Mock<IWbsApiService> wbsApiServiceMock, out Mock<IWbsCodeCacheModelService> wbsCacheServiceMock)
        {
            var configuration = _serviceProvider.GetService<IConfiguration>();

            wbsApiServiceMock = new Mock<IWbsApiService>();
            wbsApiServiceMock.Setup(m =>
            m.Exists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string wbsCode, CancellationToken cancellation) =>
                {
                    return foundInApi;
                });

            wbsCacheServiceMock = new Mock<IWbsCodeCacheModelService>();

            wbsCacheServiceMock.Setup(m =>
          m.Get(It.IsAny<string>()))
              .ReturnsAsync((string wbsCode) =>
              {
                  if (foundInCache)
                  {
                      return new WbsCodeCache(wbsCode, validInCache, DateTime.UtcNow.AddMinutes(10));
                  }
                  else
                  {
                      return null;
                  }
              });

            wbsCacheServiceMock.Setup(m => m.Add(It.IsAny<string>(), It.IsAny<bool>()));
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);
            return new WbsValidationService(
                configuration,
                 userService.Object,
                 OperationPermissionServiceMockFactory.Create(userService.Object),
                 wbsApiServiceMock.Object,
                 wbsCacheServiceMock.Object
                );
        }
    }
}
