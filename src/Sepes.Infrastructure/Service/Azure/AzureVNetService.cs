﻿using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Service.Azure.Interface;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class AzureVNetService : AzureServiceBase, IAzureVNetService
    { 
        public AzureVNetService(IConfiguration config, ILogger<AzureVNetService> logger)
            :base (config, logger)
        {         
          
        }

        public async Task<CloudResourceCRUDResult> EnsureCreatedAndConfigured(CloudResourceCRUDInput parameters)
        {
            _logger.LogInformation($"Creating Network for sandbox with Name: {parameters.SandboxName}! Resource Group: {parameters.ResourceGrupName}");

            var networkSettings = SandboxResourceConfigStringSerializer.NetworkSettings(parameters.CustomConfiguration);

            var vNetDto = await GetResourceWrappedInDtoAsync(parameters.ResourceGrupName, parameters.Name);

            if (vNetDto == null)
            {
                vNetDto = await CreateAsync(parameters.Region, parameters.ResourceGrupName, parameters.Name, networkSettings.SandboxSubnetName, parameters.Tags);
            }
            else
            {
                throw new NotImplementedException("Update Network not implemented");
            }

          
            var crudResult = CreateResult(vNetDto);

            _logger.LogInformation($"Applying NSG to subnet for sandbox: {parameters.SandboxName}");

            string networkSecurityGroupName = null; //Comes from Network Security Group Service 

            if (parameters.TryGetSharedVariable(AzureCrudSharedVariable.NETWORK_SECURITY_GROUP_NAME, out networkSecurityGroupName) == false)
            {
                throw new ArgumentException("AzureVNetService: Missing Network security group name from input");
            }

            await ApplySecurityGroup(parameters.ResourceGrupName, networkSecurityGroupName, vNetDto.SandboxSubnetName, vNetDto.Network.Name);       

            _logger.LogInformation($"Done creating Network and Applying NSG for sandbox with Name: {parameters.SandboxName}! Id: {vNetDto.Id}");

            return crudResult;
        }

        public async Task<CloudResourceCRUDResult> GetSharedVariables(CloudResourceCRUDInput parameters)
        {
            var vNetDto = await GetResourceWrappedInDtoAsync(parameters.ResourceGrupName, parameters.Name);
            var crudResult = CreateResult(vNetDto);
            return crudResult;

        }

        CloudResourceCRUDResult CreateResult(AzureVNetDto networkDto)
        {
            var crudResult = CloudResourceCRUDUtil.CreateResultFromIResource(networkDto.Network);
            crudResult.CurrentProvisioningState = networkDto.ProvisioningState;
            crudResult.NewSharedVariables.Add(AzureCrudSharedVariable.BASTION_SUBNET_ID, networkDto.BastionSubnetId);
            return crudResult;
        }

        public async Task<AzureVNetDto> CreateAsync(Region region, string resourceGroupName, string networkName, string sandboxSubnetName, Dictionary<string, string> tags)
        {
            var networkDto = new AzureVNetDto();          

            var addressSpace = "10.100.0.0/23";  //Can have 512 adresses, but must reserve some; 10.100.0.0-10.100.1.255

            var bastionSubnetName = "AzureBastionSubnet";
            var bastionSubnetAddress = "10.100.0.0/24"; //Can only use 256 adress, so max is 10.100.0.255         

            networkDto.SandboxSubnetName = sandboxSubnetName;
            var sandboxSubnetAddress = "10.100.1.0/24";

            networkDto.Network = await _azure.Networks.Define(networkName)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroupName)
                
                .WithAddressSpace(addressSpace)
                .WithSubnet(bastionSubnetName, bastionSubnetAddress)
                .WithSubnet(networkDto.SandboxSubnetName, sandboxSubnetAddress)  
                .WithTags(tags)
                .CreateAsync();

            networkDto.ProvisioningState = networkDto.Network.Inner.ProvisioningState.ToString();

            return networkDto;
        }

        public async Task ApplySecurityGroup(string resourceGroupName, string securityGroupName, string subnetName, string networkName)
        {
            //Add the security group to a subnet.
            var nsg = await _azure.NetworkSecurityGroups.GetByResourceGroupAsync(resourceGroupName, securityGroupName);
            var network = await _azure.Networks.GetByResourceGroupAsync(resourceGroupName, networkName);

            //Ensure resource is is managed by this instance
            CheckIfResourceHasCorrectManagedByTagThrowIfNot(resourceGroupName, nsg.Tags);
            CheckIfResourceHasCorrectManagedByTagThrowIfNot(resourceGroupName, network.Tags);

            await network.Update()
                .UpdateSubnet(subnetName)
                .WithExistingNetworkSecurityGroup(nsg)
                .Parent()
                .ApplyAsync();
        }      

        public async Task Delete(string resourceGroupName, string networkName)
        {
            var network = await _azure.Networks.GetByResourceGroupAsync(resourceGroupName, networkName);
            CheckIfResourceHasCorrectManagedByTagThrowIfNot(resourceGroupName, network.Tags);
            await _azure.Networks.DeleteByResourceGroupAsync(resourceGroupName, networkName);
        }

        public async Task<INetwork> GetResourceAsync(string resourceGroupName, string resourceName)
        {
            var resource = await _azure.Networks.GetByResourceGroupAsync(resourceGroupName, resourceName);
            return resource;
        }

        async Task<AzureVNetDto> GetResourceWrappedInDtoAsync(string resourceGroupName, string resourceName)
        {
            var resource = await _azure.Networks.GetByResourceGroupAsync(resourceGroupName, resourceName);

            if(resource == null)
            {
                return null;
            }

            var dto = new AzureVNetDto() { Network = resource, ProvisioningState = resource.Inner.ProvisioningState.Value };

            return dto;
        }


        public async Task<string> GetProvisioningState(string resourceGroupName, string resourceName)
        {
            var resource = await GetResourceAsync(resourceGroupName, resourceName);

            if (resource == null)
            {
                throw NotFoundException.CreateForAzureResource(resourceName, resourceGroupName);
            }

            return resource.Inner.ProvisioningState.ToString();
        }

        public async Task<IDictionary<string, string>> GetTagsAsync(string resourceGroupName, string resourceName)
        {
            var vNet = await GetResourceAsync(resourceGroupName, resourceName);
            return AzureResourceTagsFactory.TagReadOnlyDictionaryToDictionary(vNet.Tags);
        }

        public async Task UpdateTagAsync(string resourceGroupName, string resourceName, KeyValuePair<string, string> tag)
        {
            var resource = await GetResourceAsync(resourceGroupName, resourceName);

            CheckIfResourceHasCorrectManagedByTagThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.UpdateTags().WithoutTag(tag.Key).ApplyTagsAsync();
            _ = await resource.UpdateTags().WithTag(tag.Key, tag.Value).ApplyTagsAsync();
        }

        public Task<CloudResourceCRUDResult> Delete(CloudResourceCRUDInput parameters)
        {
            throw new NotImplementedException();
        }

       


        //public async Task<INetwork> Create(Region region, string resourceGroupName, string studyName, string sandboxName)
        //{
        //    var networkName = CreateVNetName(studyName, sandboxName);

        //    var addressSpace = "10.100.10.0/23"; // Until 10.100.11.255 Can have 512 adresses, but must reserve some;

        //    var bastionSubnetName = "AzureBastionSubnet";
        //    var bastionSubnetAddress = "10.100.0.0/24"; //Can only use 256 adress, so max is 10.100.0.255

        //    var sandboxSubnetName = $"snet-{sandboxName}";
        //    var sandboxSubnetAddress = "10.100.1.0/24";

        //    var network = await _azure.Networks.Define(networkName)
        //        .WithRegion(region)
        //        .WithExistingResourceGroup(resourceGroupName)
        //        .WithAddressSpace(addressSpace)
        //        .WithSubnet(bastionSubnetName, bastionSubnetAddress)
        //        .WithSubnet(sandboxSubnetName, sandboxSubnetAddress)
        //        .CreateAsync();
        //    using (NetworkManagementClient client = new NetworkManagementClient(credentials))
        //    {

        //        VirtualNetworkInner vnet = new VirtualNetworkInner()
        //    {
        //        Location = "West US",
        //        AddressSpace = new AddressSpace()
        //        {
        //            AddressPrefixes = new List<string>() { "0.0.0.0/16" }
        //        },

        //        DhcpOptions = new DhcpOptions()
        //        {
        //            DnsServers = new List<string>() { "1.1.1.1", "1.1.2.4" }
        //        },

        //        Subnets = new List<Subnet>()
        //{
        //    new Subnet()
        //    {
        //        Name = subnet1Name,
        //        AddressPrefix = "1.0.1.0/24",
        //    },
        //    new Subnet()
        //    {
        //        Name = subnet2Name,
        //       AddressPrefix = "1.0.2.0/24",
        //    }
        //}
        //    };

        //    await client.VirtualNetworks.CreateOrUpdateAsync(resourceGroupName, vNetName, vnet);
        //    }
        //    return network;
        //}
    }
}
