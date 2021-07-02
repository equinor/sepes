using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class WbsCodeCacheModelService : IWbsCodeCacheModelService
    {
        const string WBS_TABLE_NAME = "dbo.[WbsCodeCache]";

        readonly ILogger _logger;
        readonly IDapperQueryService _dapperQueryService;

        public WbsCodeCacheModelService(ILogger<WbsCodeCacheModelService> logger, IDapperQueryService dapperQueryService)
        {
            _logger = logger;
            _dapperQueryService = dapperQueryService;
        }

        public async Task<WbsCodeCache> Get(string wbsCode)
        {
            try
            {
                _logger.LogInformation($"Wbs Cache - Get: {wbsCode}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Wbs Cache - Get: {wbsCode}, failed");
            }

            return await GetInternal(wbsCode);
        }

        async Task<WbsCodeCache> GetInternal(string wbsCode)
        {
            var getWbsQuery = $"{BasicQueryProjection()} AND {ExpiresPart()}";
            return await RunSingleWbsCodeQuery<WbsCodeCache>(getWbsQuery, wbsCode);
        }

        async Task<T> RunSingleWbsCodeQuery<T>(string query, string wbsCode)
        {
            try
            {
                return await _dapperQueryService.RunDapperQuerySingleAsync<T>(query, new { wbsCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"The following query failed: {query}, wbsCode: {wbsCode}");
            }

            return default;
        }

        public async Task Add(string wbsCode, bool valid)
        {
            _logger.LogInformation($"Wbs Cache - Add: {wbsCode}, valid: {valid}");

            try
            {
                var validString = valid ? "1" : "0";

                if (await WbsCodeExists(wbsCode))
                {
                    _logger.LogInformation($"Wbs Cache - Add: {wbsCode}, valid: {valid}, item exists allready");

                    var updateEntrySql = $"UPDATE {WBS_TABLE_NAME} SET [Valid] = {validString}, [Expires] = {GetNewExpiresSql(valid)} {WhereEqualsWbsCodePart()}";

                    await _dapperQueryService.ExecuteAsync(updateEntrySql, new { wbsCode });
                }
                else
                {
                    _logger.LogInformation($"Wbs Cache - Add: {wbsCode}, valid: {valid}, item does not exist, adding!");
                    var insertWbsCodeSql = $"IF NOT EXISTS({AnyQuery()}) BEGIN INSERT INTO {WBS_TABLE_NAME} ([WbsCode],[Valid],[Expires]) VALUES (@wbsCode,{validString},{GetNewExpiresSql(valid)}) END";

                    await _dapperQueryService.ExecuteAsync(insertWbsCodeSql, new { wbsCode });
                }
            }
            catch (Exception ex)
            {
                //throw new CustomUserMessageException($"Unable to add wbs cache entry {wbsCode}, valid: {valid}", ex, $"Failed to update WBS cache");
                _logger.LogError(ex, $"Unable to add wbs cache entry {wbsCode}, valid: {valid}");
            }
        }    

        async Task<bool> WbsCodeExists(string wbsCode)
        {
            var getWbsQuery = $"{AnyQueryAsBool()}";
            return await RunSingleWbsCodeQuery<bool>(getWbsQuery, wbsCode);
        }

        public async Task Clean()
        {
            var cleanWbsQuery = $"DELETE FROM {WBS_TABLE_NAME} WHERE {ExpiresPart()}";
            await _dapperQueryService.ExecuteAsync(cleanWbsQuery);
        }

        string AnyQuery()
        {
            return $"SELECT [WbsCode] FROM {WBS_TABLE_NAME} WHERE [WbsCode] = @wbsCode";
        }

        string AnyQueryAsBool()
        {
            return $"SELECT CASE WHEN EXISTS({AnyQuery()}) THEN 1 ELSE 0 END";
        }

        string BasicQueryProjection()
        {
            return $"SELECT [WbsCode],[Valid],[Expires] FROM {WBS_TABLE_NAME} {WhereEqualsWbsCodePart()}";
        }

        string WhereEqualsWbsCodePart()
        {
            return "WHERE [WbsCode] = @wbsCode";
        }

        string ExpiresPart()
        {
            return "[Expires] > GETUTCDATE()";
        }

        DateTime GetNewExpires(bool valid)
        {
            return DateTime.UtcNow.AddMinutes(valid ? 20 : 3);
        }

        string GetNewExpiresSql(bool valid)
        {           
            var expires = GetNewExpires(valid);        
            var expiresString = expires.ToString("s");
            return $"'{expiresString}'";
        }
    }
}