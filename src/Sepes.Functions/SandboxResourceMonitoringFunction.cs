using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Sepes.Provisioning.Service.Interface;

namespace Sepes.Functions
{
    public class SandboxResourceMonitoringFunction
    {
        readonly ICloudResourceMonitoringService _resourceMonitoringService;

        public SandboxResourceMonitoringFunction(ICloudResourceMonitoringService resourceMonitoringService)
        {
            _resourceMonitoringService = resourceMonitoringService;
        }

        //To run every minute (in debug only): "0 */30 * * * *"
        //Run every hour: "0 * * * *"        
        [FunctionName("SandboxResourceMonitoring")]
        public async Task Run([TimerTrigger("0 * * * * *", RunOnStartup =true)]TimerInfo myTimer, ILogger log)

        {
           await _resourceMonitoringService.StartMonitoringSession();
        }       
    }
}
