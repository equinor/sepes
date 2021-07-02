using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Service.Interface;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Provisioning;
using Sepes.Common.Exceptions;
using Sepes.Common.Util;

namespace Sepes.Azure.Service
{
    public class AzureVirtualNetworkService : AzureServiceBase, IAzureVirtualNetworkService
    {
        public AzureVirtualNetworkService(IConfiguration config, ILogger<AzureVirtualNetworkService> logger)
            : base(config, logger)
        {

        }

        public async Task<ResourceProvisioningResult> EnsureCreated(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Creating virtual network {parameters.Name} in resource Group: {parameters.ResourceGroupName}");

            var networkSettings = CloudResourceConfigStringSerializer.NetworkSettings(parameters.ConfigurationString);

            var virtualNetwork = await GetResourceInternalAsync(parameters.ResourceGroupName, parameters.Name, false);           

            if (virtualNetwork == null)
            {
                virtualNetwork = await CreateInternalAsync(GetRegionFromString(parameters.Region), parameters.ResourceGroupName, parameters.Name, networkSettings.SandboxSubnetName, parameters.Tags, cancellationToken);
            }          

            if (!parameters.TryGetSharedVariable(AzureCrudSharedVariable.NETWORK_SECURITY_GROUP_NAME, out string networkSecurityGroupName))
            {
                throw new ArgumentException("AzureVirtualNetworkService: Missing network security group name from input");
            }
            
            await EnsureNetworkSecurityGroupIsAddedToSubnet(virtualNetwork, networkSecurityGroupName, networkSettings.SandboxSubnetName);

            _logger.LogInformation($"Done creating virtual network {parameters.Name}");

            var crudResult = CreateResult(virtualNetwork);
            return crudResult;
        }

        public async Task<ResourceProvisioningResult> GetSharedVariables(ResourceProvisioningParameters parameters)
        {
            var network = await GetResourceInternalAsync(parameters.ResourceGroupName, parameters.Name);
            var crudResult = CreateResult(network);
            return crudResult;
        }

        ResourceProvisioningResult CreateResult(INetwork network)
        {
            var crudResult = ResourceProvisioningResultUtil.CreateFromIResource(network);
            crudResult.CurrentProvisioningState = network.Inner.ProvisioningState.ToString();
            crudResult.NewSharedVariables.Add(AzureCrudSharedVariable.BASTION_SUBNET_ID, AzureVNetUtil.GetBastionSubnetId(network));
            return crudResult;
        }

        async Task<INetwork> CreateInternalAsync(Region region, string resourceGroupName, string networkName, string sandboxSubnetName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var addressSpace = "10.100.0.0/23";  //Can have 512 adresses, but must reserve some; 10.100.0.0-10.100.1.255
                       
            var bastionSubnetAddress = "10.100.0.0/24"; //Can only use 256 adress, so max is 10.100.0.255
            var sandboxSubnetAddress = "10.100.1.0/24";

            var network = await _azure.Networks.Define(networkName)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroupName)
                .WithAddressSpace(addressSpace)
                .WithSubnet(AzureVNetConstants.BASTION_SUBNET_NAME, bastionSubnetAddress)
                .DefineSubnet(sandboxSubnetName).WithAddressPrefix(sandboxSubnetAddress).WithAccessFromService(ServiceEndpointType.MicrosoftStorage).Attach() //To allow storage accounts to be added                                                                                                                                                               
                .WithTags(tags)
                .CreateAsync(cancellationToken);           

            return network;
        }

        public async Task EnsureSandboxSubnetHasServiceEndpointForStorage(string resourceGroupName, string networkName)
        {
            var network = await _azure.Networks.GetByResourceGroupAsync(resourceGroupName, networkName);

            //Ensure resource is is managed by this instance         
            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, network.Tags);

            var sandboxSubnet = AzureVNetUtil.GetSandboxSubnetOrThrow(network);

            await network.Update()
                .UpdateSubnet(sandboxSubnet.Name)
                .WithAccessFromService(ServiceEndpointType.MicrosoftStorage)
                .Parent()
                .ApplyAsync();
        }    

        async Task EnsureNetworkSecurityGroupIsAddedToSubnet(INetwork network, string securityGroupName, string sandboxSubnetName)
        {
            _logger.LogInformation($"Ensuring network security group {securityGroupName} is added to subnet {sandboxSubnetName} for network {network.Name}");

            var nsg = await _azure.NetworkSecurityGroups.GetByResourceGroupAsync(network.ResourceGroupName, securityGroupName);

            if(nsg == null)
            {
                throw new Exception($"Network security group {securityGroupName} not found in {network.ResourceGroupName}");
            }

            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(network.ResourceGroupName, nsg.Tags);

            var sandboxSubnet = network.Subnets[sandboxSubnetName];

            if(sandboxSubnet == null)
            {
                throw new Exception($"Sandbox subnet {sandboxSubnetName} not found for network {network.Name} in resource group {network.ResourceGroupName}");
            }

            if (String.IsNullOrWhiteSpace(network.Subnets[sandboxSubnetName].NetworkSecurityGroupId))
            {
                _logger.LogInformation($"Network security group {securityGroupName} not added to subnet {sandboxSubnetName}, adding");

                await network.Update()
                                .UpdateSubnet(sandboxSubnetName)
                                .WithExistingNetworkSecurityGroup(nsg)
                                .Parent()
                                .ApplyAsync();
            }
            else
            {
               _logger.LogInformation($"Network security group {nsg.Name} allready applied to subnet {sandboxSubnetName} for network {network.Name} in resource group {network.ResourceGroupName}");
            }            
        }

        async Task<INetwork> GetResourceInternalAsync(string resourceGroupName, string resourceName, bool failIfNotFound = true)
        {
            var resource = await _azure.Networks.GetByResourceGroupAsync(resourceGroupName, resourceName);

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
            var vNet = await GetResourceInternalAsync(resourceGroupName, resourceName);
            return TagUtils.TagReadOnlyDictionaryToDictionary(vNet.Tags);
        }

        public async Task UpdateTagAsync(string resourceGroupName, string resourceName, KeyValuePair<string, string> tag, CancellationToken cancellationToken = default)
        {
            var resource = await GetResourceInternalAsync(resourceGroupName, resourceName);

            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.UpdateTags().WithoutTag(tag.Key).ApplyTagsAsync(cancellationToken);
            _ = await resource.UpdateTags().WithTag(tag.Key, tag.Value).ApplyTagsAsync(cancellationToken);
        }

        public async Task SetTagsAsync(string resourceGroupName, string resourceName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var resource = await GetResourceInternalAsync(resourceGroupName, resourceName);

            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.Update().WithTags(tags).ApplyAsync(cancellationToken);
        }

        public Task<ResourceProvisioningResult> EnsureDeleted(ResourceProvisioningParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ResourceProvisioningResult> Update(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
