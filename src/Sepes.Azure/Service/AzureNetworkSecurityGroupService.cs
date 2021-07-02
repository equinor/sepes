using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Service.Interface;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Provisioning;
using Sepes.Common.Exceptions;

namespace Sepes.Azure.Service
{
    public class AzureNetworkSecurityGroupService : AzureServiceBase, IAzureNetworkSecurityGroupService
    {
        public AzureNetworkSecurityGroupService(IConfiguration config, ILogger<AzureNetworkSecurityGroupService> logger)
             : base(config, logger)
        {


        }

        public async Task<ResourceProvisioningResult> EnsureCreated(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Ensuring Network Security Group exists for sandbox with Name: {parameters.SandboxName}! Resource Group: {parameters.ResourceGroupName}");
          
            var nsg = await GetResourceInternalAsync(parameters.ResourceGroupName, parameters.Name, false);

            if (nsg == null)
            {
                _logger.LogInformation($"Network Security Group not found for sandbox with Name: {parameters.SandboxName}! Resource Group: {parameters.ResourceGroupName}. Creating!");

                nsg = await CreateInternal(GetRegionFromString(parameters.Region), parameters.ResourceGroupName, parameters.Name, parameters.Tags, cancellationToken);
            }

            var result = CreateResult(nsg);

            _logger.LogInformation($"Done ensuring Network Security Group exists for sandbox with Id: {parameters.SandboxName}! Id: {nsg.Id}");

            return result;
        }

        public async Task<ResourceProvisioningResult> GetSharedVariables(ResourceProvisioningParameters parameters)
        {
            var nsg = await GetResourceInternalAsync(parameters.ResourceGroupName, parameters.Name);

            var result = CreateResult(nsg);

            return result;
        }

        public async Task<ResourceProvisioningResult> EnsureDeleted(ResourceProvisioningParameters parameters)
        {
            await EnsureDeletedInternalAsync(parameters.ResourceGroupName, parameters.Name);
            var provisioningState = await GetProvisioningState(parameters.ResourceGroupName, parameters.Name);
            var crudResult = ResourceProvisioningResultUtil.CreateFromProvisioningState(provisioningState);
            return crudResult;
        }

        ResourceProvisioningResult CreateResult(INetworkSecurityGroup nsg)
        {
            var crudResult = ResourceProvisioningResultUtil.CreateFromIResource(nsg);
            crudResult.CurrentProvisioningState = nsg.Inner.ProvisioningState.ToString();
            crudResult.NewSharedVariables.Add(AzureCrudSharedVariable.NETWORK_SECURITY_GROUP_NAME, nsg.Name);
            return crudResult;
        }

        async Task<INetworkSecurityGroup> CreateInternal(Region region, string resourceGroupName, string nsgName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var nsg = await _azure.NetworkSecurityGroups
                .Define(nsgName)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroupName)
                .WithTags(tags)
                .CreateAsync(cancellationToken);


            await NsgApplyBaseRules(nsg);

            return nsg;
        }

        async Task NsgApplyBaseRules(INetworkSecurityGroup nsg)
        {
            await nsg.Update()
            .DefineRule(AzureVmConstants.RulePresets.ALLOW_FOR_SERVICETAG_VNET)
            .AllowOutbound()
            .FromAnyAddress()
            .FromAnyPort()
            .ToAddress("VirtualNetwork")
            .ToAnyPort()
            .WithAnyProtocol()
            .WithPriority(AzureVmConstants.RulePresets.ALLOW_FOR_SERVICETAG_VNET_PRIORITY)
            .Attach()
            .ApplyAsync();
        }

        async Task EnsureDeletedInternalAsync(string resourceGroupName, string resourceName)
        {
            var resource = await GetResourceInternalAsync(resourceGroupName, resourceName, false);

            if (resource == null)
            {
                //Allready deleted
                _logger.LogWarning($"Deleting resource {resourceName} failed because it was not found. Assuming allready deleted");
                return;
            }         

            //Ensure resource is is managed by this instance
            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            await _azure.NetworkSecurityGroups.DeleteByResourceGroupAsync(resourceGroupName, resourceName);
        }       

        public async Task<INetworkSecurityGroup> GetResourceInternalAsync(string resourceGroupName, string resourceName, bool failIfNotFound = true)
        {
            var resource = await _azure.NetworkSecurityGroups.GetByResourceGroupAsync(resourceGroupName, resourceName);

            if (resource == null)
            {
                if (failIfNotFound)
                {
                    throw NotFoundException.CreateForAzureResource(resourceName, resourceGroupName);
                }
                else
                {
                    return null;
                }
            }

            return resource;
        }

        public async Task<string> GetProvisioningState(string resourceGroupName, string resourceName)
        {
            var resource = await GetResourceInternalAsync(resourceGroupName, resourceName, false);

            if (resource == null)
            {
                return null;
            }

            return resource.Inner.ProvisioningState.ToString();
        }

        public async Task<IDictionary<string, string>> GetTagsAsync(string resourceGroupName, string resourceName)
        {
            var rg = await GetResourceInternalAsync(resourceGroupName, resourceName);
            return TagUtils.TagReadOnlyDictionaryToDictionary(rg.Tags);
        }

        public async Task UpdateTagAsync(string resourceGroupName, string resourceName, KeyValuePair<string, string> tag, CancellationToken cancellationToken = default)
        {
            var resource = await GetResourceInternalAsync(resourceGroupName, resourceName);
        
            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.UpdateTags().WithoutTag(tag.Key).ApplyTagsAsync();
            _ = await resource.UpdateTags().WithTag(tag.Key, tag.Value).ApplyTagsAsync();
        }

        public async Task SetTagsAsync(string resourceGroupName, string resourceName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var resource = await GetResourceInternalAsync(resourceGroupName, resourceName);

            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.Update().WithTags(tags).ApplyAsync(cancellationToken);
        }        
        
        public Task<ResourceProvisioningResult> Update(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
