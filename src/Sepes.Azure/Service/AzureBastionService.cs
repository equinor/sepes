using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.Network.Models;
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
    public class AzureBastionService : AzureServiceBase, IAzureBastionService
    {
        public AzureBastionService(IConfiguration config, ILogger<AzureBastionService> logger) : base(config, logger)
        {

        }

        public async Task<ResourceProvisioningResult> EnsureCreated(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Creating Bastion for sandbox with Name: {parameters.SandboxName}! Resource Group: {parameters.ResourceGroupName}");

            if (!parameters.TryGetSharedVariable(AzureCrudSharedVariable.BASTION_SUBNET_ID, out string subnetId))
            {
                throw new ArgumentException("AzureBastionService: Missing Bastion subnet ID from input");
            }

            var bastionHost = await GetResourceInternalAsync(parameters.ResourceGroupName, parameters.Name, false);

            if (bastionHost == null)
            {
                bastionHost = await CreateInternal(GetRegionFromString(parameters.Region), parameters.ResourceGroupName, parameters.Name, subnetId, parameters.Tags, cancellationToken);
                _logger.LogInformation($"Done creating Bastion for sandbox with Id: {parameters.SandboxName}! Bastion Id: {bastionHost.Id}");
            }
            else
            {
                _logger.LogInformation($"Ensure bastion exist for Sandbox: {parameters.SandboxName}! Bastion allready existed. Bastion Id: {bastionHost.Id}");
            }

            var result = CreateResult(bastionHost);

            return result;
        }

        public async Task<ResourceProvisioningResult> GetSharedVariables(ResourceProvisioningParameters parameters)
        {
            var bastion = await GetResourceInternalAsync(parameters.ResourceGroupName, parameters.Name);
            var result = CreateResult(bastion);
            return result;
        }



        public async Task<ResourceProvisioningResult> EnsureDeleted(ResourceProvisioningParameters parameters)
        {
            await DeleteInternal(parameters.ResourceGroupName, parameters.Name);
            var provisioningState = await GetProvisioningState(parameters.ResourceGroupName, parameters.Name);
            var crudResult = ResourceProvisioningResultUtil.CreateFromProvisioningState(provisioningState);
            return crudResult;
        }


        async Task<BastionHost> CreateInternal(Region region, string resourceGroupName, string bastionName, string subnetId, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var publicIpName = AzureResourceNameUtil.BastionPublicIp(bastionName);

            var pip = await _azure.PublicIPAddresses.Define(publicIpName)
             .WithRegion(region)
             .WithExistingResourceGroup(resourceGroupName)
             .WithStaticIP()
             .WithSku(PublicIPSkuType.Standard)
             .WithTags(tags)
             .CreateAsync(cancellationToken);

            using (var client = new NetworkManagementClient(_credentials))
            {
                client.SubscriptionId = _subscriptionId;

                var ipConfigs = new List<BastionHostIPConfiguration> { new BastionHostIPConfiguration()
                        {
                            Name = $"{bastionName}-ip-config",
                            Subnet =  new SubResource(subnetId),
                            PrivateIPAllocationMethod = "Dynamic",
                            PublicIPAddress = new SubResource(pip.Inner.Id),
                        }
                    };

                var bastion = new BastionHost()
                {
                    Location = region.Name,
                    IpConfigurations = ipConfigs,
                    Tags = tags
                };

                var createdBastion = await client.BastionHosts.CreateOrUpdateAsync(resourceGroupName, bastionName, bastion, cancellationToken);

                return createdBastion;
            }
        }


        public async Task<string> GetProvisioningState(string resourceGroupName, string bastionHostName)
        {
            var bastion = await GetResourceInternalAsync(resourceGroupName, bastionHostName, false);

            if (bastion == null)
            {
                return null;
            }

            return bastion.ProvisioningState;
        }

        public async Task<IDictionary<string, string>> GetTagsAsync(string resourceGroupName, string resourceName)
        {
            var rg = await GetResourceInternalAsync(resourceGroupName, resourceName);
            return rg.Tags;
        }

        public async Task UpdateTagAsync(string resourceGroupName, string resourceName, KeyValuePair<string, string> tag, CancellationToken cancellationToken = default)
        {
            var bastion = await GetResourceInternalAsync(resourceGroupName, resourceName);

            CheckIfResourceHasCorrectManagedByTagThrowIfNot(resourceGroupName, bastion.Tags);

            bastion.Tags[tag.Key] = tag.Value;

            await UpdateResourceInternal(resourceGroupName, bastion, cancellationToken);
        }

        public async Task SetTagsAsync(string resourceGroupName, string resourceName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var bastion = await GetResourceInternalAsync(resourceGroupName, resourceName);

            CheckIfResourceHasCorrectManagedByTagThrowIfNot(resourceGroupName, bastion.Tags);

            bastion.Tags = tags;

            await UpdateResourceInternal(resourceGroupName, bastion, cancellationToken);

            await UpdatePublicIpTags(resourceGroupName, bastion, tags, cancellationToken);
        }

        async Task UpdateResourceInternal(string resourceGroupName, BastionHost bastion, CancellationToken cancellationToken = default)
        {
            using (var client = new NetworkManagementClient(_credentials))
            {
                client.SubscriptionId = _subscriptionId;
                await client.BastionHosts.CreateOrUpdateAsync(resourceGroupName, bastion.Name, bastion, cancellationToken);
            }
        }

        async Task UpdatePublicIpTags(string resourceGroupName, BastionHost bastion, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            foreach (var currentIpConfig in bastion.IpConfigurations)
            {
                try
                {
                    var pip = await _azure.PublicIPAddresses.GetByIdAsync(currentIpConfig.PublicIPAddress.Id, cancellationToken);

                    if (pip != null)
                    {
                        await _azure.PublicIPAddresses.Inner.UpdateTagsWithHttpMessagesAsync(resourceGroupName, pip.Name, tags, cancellationToken: cancellationToken);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to update public IP tags for Bastion {bastion.Name}");
                }
            }
        }

        public Task<ResourceProvisioningResult> Update(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        ResourceProvisioningResult CreateResult(BastionHost bastion)
        {
            var crudResult = ResourceProvisioningResultUtil.CreateFromIResource(bastion);
            crudResult.CurrentProvisioningState = bastion.ProvisioningState.ToString();
            return crudResult;
        }

        async Task<BastionHost> GetResourceInternalAsync(string resourceGroupName, string bastionHostName, bool failIfNotFound = true)
        {
            try
            {
                using (var client = new NetworkManagementClient(_credentials))
                {
                    client.SubscriptionId = _subscriptionId;
                    var bastion = await client.BastionHosts.GetAsync(resourceGroupName, bastionHostName);
                    return bastion;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("could not be found") || ex.Message.ToLower().Contains("was not found"))
                {
                    if (failIfNotFound)
                    {
                        throw NotFoundException.CreateForAzureResource(bastionHostName, resourceGroupName, ex);
                    }
                    else
                    {
                        return null;
                    }
                }

                throw;
            }
        }
        async Task DeleteInternal(string resourceGroupName, string bastionHostName)
        {
            var bastion = await GetResourceInternalAsync(resourceGroupName, bastionHostName, false);

            if (bastion == null)
            {
                //Allready deleted
                _logger.LogWarning($"Deleting resource {bastionHostName} failed because it was not found. Assuming allready deleted");
                return;
            }

            using (var client = new NetworkManagementClient(_credentials))
            {
                client.SubscriptionId = _subscriptionId;
                CheckIfResourceHasCorrectManagedByTagThrowIfNot(resourceGroupName, bastion.Tags);
                await client.BastionHosts.DeleteAsync(resourceGroupName, bastionHostName);
            }
        }


    }

}

