﻿using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class AzureResourceGroupService : AzureServiceBase, IAzureResourceGroupService
    { 
        
        public AzureResourceGroupService(IConfiguration config, ILogger logger)
            :base(config, logger)
        {
        }

      

        public async Task<IResourceGroup> CreateForStudy(string studyName, string sandboxName, Region region, Dictionary<string, string> tags)
        {
            string resourceGroupName = AzureResourceNameUtil.ResourceGroupForStudy(sandboxName);

            //TODO: Add tags, where to get?
            //TechnicalContact (Specified per sandbox?)
            //TechnicalContactEmail (Specified per sandbox?)
            //Sponsor
            //SponsorEmail

            return await Create(resourceGroupName, region, tags);         
        }

        public async Task<IResourceGroup> Create(string resourceGroupName, Region region, Dictionary<string, string> tags)
        {
            IResourceGroup resourceGroup = await _azure.ResourceGroups
                    .Define(resourceGroupName)
                    .WithRegion(region)
                    .WithTags(tags)
                    .CreateAsync();     
            
            return resourceGroup;
        }       

        public async Task<bool> Exists(string resourceGroupName)
        {
            return await _azure.ResourceGroups.ContainAsync(resourceGroupName);
        }      

        public async Task Delete(string resourceGroupName)
        {
            await _azure.ResourceGroups.DeleteByNameAsync(resourceGroupName);
        }

    }
}
