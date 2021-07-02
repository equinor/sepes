using System;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Sepes.Common.Constants;
using Sepes.Common.Util;

namespace Sepes.Azure.Util
{
    public static class AzureResourceUtil
    {     

        public static void ThrowIfResourceIsNull(IResource resource, string resourceType, string name, string errorMessagePrefix)
        {
            if (resource == null)
            {
                throw new Exception($"{errorMessagePrefix}: Resource {resourceType} with name {name} was not found");
            }
        }

        public static string CreateResourceLink(IConfiguration config, string resourceId)
        {
            if (String.IsNullOrWhiteSpace(resourceId))
            {
                return null;
            }           

            if (resourceId == AzureResourceNameUtil.AZURE_RESOURCE_INITIAL_ID_OR_NAME)
            {
                return null;
            }

            var domain = ConfigUtil.GetConfigValueAndThrowIfEmpty(config, ConfigConstants.AZ_DOMAIN);

            return CreateResourceLink(domain, resourceId);
        }

        public static string CreateResourceLink(string domain, string resourceId)
        {
            var azureUrlPart = "https://portal.azure.com/#@";
            return $"{azureUrlPart}{domain}/resource{resourceId}";
        }

        public static string CreateResourceRetryLink(int resourceId)
        {
            return $"api/resources/{resourceId}/retry";
        }
        
        public static string CreateResourceCostLink(string domain, string resourceId)
        {
            var azureUrlPart = "https://portal.azure.com/#@";
            return $"{azureUrlPart}{domain}/resource{resourceId}/costanalysis";
        }
    }
}
