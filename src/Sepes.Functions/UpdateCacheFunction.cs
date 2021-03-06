using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Threading.Tasks;

namespace Sepes.Functions
{
    public class UpdateCacheFunction
    {
        readonly ILogger _logger;
        readonly IConfiguration _configuration;        
        readonly IVirtualMachineSizeImportService _virtualMachineSizeImportService;
        readonly IVirtualMachineDiskSizeImportService _virtualMachineDiskSizeImportService;
        readonly IWbsCodeCacheModelService _wbsCodeCacheModelService;

        public UpdateCacheFunction(ILogger<UpdateCacheFunction> logger, IConfiguration configuration, IVirtualMachineSizeImportService virtualMachineSizeImportService, IVirtualMachineDiskSizeImportService virtualMachineDiskSizeImportService, IWbsCodeCacheModelService wbsCodeCacheModelService)
        {
            _logger = logger;
            _configuration = configuration;
            _virtualMachineSizeImportService = virtualMachineSizeImportService;
            _virtualMachineDiskSizeImportService = virtualMachineDiskSizeImportService;
            _wbsCodeCacheModelService = wbsCodeCacheModelService;
        }

        //To run every minute (in debug only): "0 */30 * * * *"
        //Run every hour: "0 * * * *"    
        //Run ever 6 hour "0 */6 * * *"
        [FunctionName("UpdateAllCaches")]
        public async Task Run([TimerTrigger("0 */6 * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            var cacheUpdateDisabled = _configuration[ConfigConstants.DISABLE_CACHE_UPDATE];

            if (!String.IsNullOrWhiteSpace(cacheUpdateDisabled) && cacheUpdateDisabled.ToLower() == "true")
            {
                _logger.LogWarning($"Cache update is disabled, aborting!");
                return;
            }

            await _virtualMachineSizeImportService.UpdateVmSizeCache();
            await _virtualMachineDiskSizeImportService.Import();
            await _wbsCodeCacheModelService.Clean();
        }
    }
}
