using Sepes.Common.Constants.CloudResource;
using Sepes.Infrastructure.Service.Interface;
using System.Threading.Tasks;

namespace Sepes.Test.Common.ServiceMocks
{
    public class DatasetWaitForFirewallOperationServiceMock : IDatasetWaitForFirewallOperationService
    {
        ICloudResourceOperationUpdateService _cloudResourceOperationUpdateService;

        public DatasetWaitForFirewallOperationServiceMock(ICloudResourceOperationUpdateService cloudResourceOperationUpdateService)

        {
            _cloudResourceOperationUpdateService = cloudResourceOperationUpdateService;          
        }

        public async Task WaitForOperationToCompleteAsync(int operationId)
        {
            await _cloudResourceOperationUpdateService.UpdateStatusAsync(operationId, CloudResourceOperationState.DONE_SUCCESSFUL);      
        }
    }
}

