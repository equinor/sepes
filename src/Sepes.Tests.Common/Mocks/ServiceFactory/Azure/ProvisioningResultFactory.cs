using Sepes.Common.Constants;
using Sepes.Common.Constants.CloudResource;
using Sepes.Common.Dto.Provisioning;

namespace Sepes.Tests.Common.ServiceMockFactories.Azure
{
    public static class ProvisioningResultFactory
    {
        public static ResourceProvisioningResult Create(ResourceProvisioningParameters provisionParameters, string resourceType)
        {
            var result = new ResourceProvisioningResult();
            result.CurrentProvisioningState = CloudResourceProvisioningStates.SUCCEEDED;
            result.IdInTargetSystem = resourceType == AzureResourceType.ResourceGroup ? $"resourceGroups/{provisionParameters.ResourceGroupName}" : $"resources/{provisionParameters.Name}";
            result.NameInTargetSystem = resourceType == AzureResourceType.ResourceGroup ? provisionParameters.ResourceGroupName : provisionParameters.Name;
         
            if(resourceType == AzureResourceType.NetworkSecurityGroup)
            {
                result.NewSharedVariables.Add(AzureCrudSharedVariable.NETWORK_SECURITY_GROUP_NAME, provisionParameters.Name);
            }
            else if(resourceType == AzureResourceType.VirtualNetwork)
            {
                result.NewSharedVariables.Add(AzureCrudSharedVariable.BASTION_SUBNET_ID, "bastionSubnetId");
            }

            return result;
        }
    }
}
