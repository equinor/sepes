﻿using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class AzureBastionService : AzureServiceBase, IAzureBastionService
    {
        public AzureBastionService(IConfiguration config, ILogger logger) : base(config, logger)
        {

        }

        public async Task<BastionHost> Create(Region region, string resourceGroupName, string studyName, string sandboxName, string subnetId)
        {
            var publicIpName = AzureResourceNameUtil.BastionPublicIp(sandboxName); // $"pip-{studyName}-{sandboxName}-bastion";

            var pip = await _azure.PublicIPAddresses.Define(publicIpName)
             .WithRegion(region)
             .WithExistingResourceGroup(resourceGroupName)
             .WithStaticIP()
             .WithSku(PublicIPSkuType.Standard)
             .CreateAsync();

            using (var client = new Microsoft.Azure.Management.Network.NetworkManagementClient(_credentials))
            {
                client.SubscriptionId = _subscriptionId;

                var bastionName = AzureResourceNameUtil.Bastion(sandboxName);

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

                };

                var createdBastion = await client.BastionHosts.CreateOrUpdateAsync(resourceGroupName, bastionName, bastion);   
                
                return createdBastion;
            }
        }

        public async Task Delete(string resourceGroupName, string bastionHostName)
        {
            using (var client = new Microsoft.Azure.Management.Network.NetworkManagementClient(_credentials))
            {
                client.SubscriptionId = _subscriptionId;
                await client.BastionHosts.DeleteAsync(resourceGroupName, bastionHostName);
            }
        }

        public async Task<bool> Exists(string resourceGroupName, string bastionHostName)
        {
            using (var client = new Microsoft.Azure.Management.Network.NetworkManagementClient(_credentials))
            {
                client.SubscriptionId = _subscriptionId;
                var bastion = await client.BastionHosts.GetAsync(resourceGroupName, bastionHostName);

                if (bastion == null)
                {
                    return false;
                }

                return !string.IsNullOrWhiteSpace(bastion.Id);
            }

        }
    }

}

