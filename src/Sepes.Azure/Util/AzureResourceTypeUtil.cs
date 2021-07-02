using Sepes.Common.Constants;

namespace Sepes.Azure.Util
{
    public static class AzureResourceTypeUtil
    {
       public static string GetUserFriendlyName(string resourceType)
        {
            switch (resourceType)
            {
                case AzureResourceType.ResourceGroup:
                    return AzureResourceTypeFriendlyName.ResourceGroup;
                case AzureResourceType.StorageAccount:
                    return AzureResourceTypeFriendlyName.StorageAccount;
                case AzureResourceType.VirtualNetwork:
                    return AzureResourceTypeFriendlyName.VirtualNetwork;
                case AzureResourceType.NetworkSecurityGroup:
                    return AzureResourceTypeFriendlyName.NetworkSecurityGroup;
                case AzureResourceType.Bastion:
                    return AzureResourceTypeFriendlyName.Bastion;
                case AzureResourceType.VirtualMachine:
                    return AzureResourceTypeFriendlyName.VirtualMachine;
                default:
                    return "n/a";
            }
        }
    }
}
