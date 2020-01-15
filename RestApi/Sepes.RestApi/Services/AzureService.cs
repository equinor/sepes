using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Threading.Tasks;
using Sepes.RestApi.Model;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Management.Network.Fluent;

namespace Sepes.RestApi.Services
{
    // Wraps call to azure. This service will most likely need to be split up into smaller servies.
    // This is (and future children) is the only code that is alloed to create and destoy azure resources.
    [ExcludeFromCodeCoverage]
    public class AzureService : IAzureService
    {
        private readonly IAzure _azure;
        private readonly string _commonResourceGroup;
        private readonly string _joinNetworkRoleName;

        public AzureService(AzureConfig config)
        {
            _commonResourceGroup = config.commonGroup;
            _azure = Azure.Authenticate(config.credentials).WithDefaultSubscription();
            _joinNetworkRoleName = "ExampleJoinNetwork";

            if (!_azure.ResourceGroups.Contain(_commonResourceGroup))
            {
                _azure.ResourceGroups
                    .Define(_commonResourceGroup)
                    .WithRegion(Region.EuropeNorth)
                    .Create();
            }
        }

        public async Task<string> CreateResourceGroup(string resourceGroupName)
        {
            //Create ResourceGroup
            var resourceGroup = await _azure.ResourceGroups
                    .Define(resourceGroupName)
                    .WithRegion(Region.EuropeNorth)
                    .CreateAsync();

            //return resource id from iresource objects
            return resourceGroup.Id;
        }
        
        public Task TerminateResourceGroup(string commonResourceGroup)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreateNetwork(string networkName, string addressSpace, string subnetName)
        {
            var network = await _azure.Networks.Define(networkName)
                .WithRegion(Region.EuropeNorth)
                .WithExistingResourceGroup(_commonResourceGroup)
                .WithAddressSpace(addressSpace).WithSubnet(subnetName, addressSpace)
                .CreateAsync();

            return network.Id;
        }

        public async Task RemoveNetwork(string vNetName)
        {
            await _azure.Networks.DeleteByResourceGroupAsync(_commonResourceGroup, vNetName);
            return;
        }

        public async Task CreateSecurityGroup(string securityGroupName)
        {
            var nsg = await _azure.NetworkSecurityGroups
                .Define(securityGroupName)
                .WithRegion(Region.EuropeNorth)
                .WithExistingResourceGroup(_commonResourceGroup)
                /*.WithTag()*/
                .CreateAsync();

            //Add rules obligatory to every pod. This will block AzureLoadBalancer from talking to the VMs inside sandbox
            await this.NsgApplyBaseRules(nsg);
        }

        public async Task DeleteSecurityGroup(string securityGroupName)
        {
            await _azure.NetworkSecurityGroups.DeleteByResourceGroupAsync(_commonResourceGroup, securityGroupName);
        }
        
        public async Task ApplySecurityGroup(string securityGroupName, string subnetName, string networkName)
        {
            //Add the security group to a subnet.
            var nsg = _azure.NetworkSecurityGroups.GetByResourceGroup(_commonResourceGroup, securityGroupName);
            var network = _azure.Networks.GetByResourceGroup(_commonResourceGroup, networkName);
            await network.Update()
                .UpdateSubnet(subnetName)
                .WithExistingNetworkSecurityGroup(nsg)
                .Parent()
                .ApplyAsync();
        }
        
        public async Task RemoveSecurityGroup(string subnetName, string networkName)
        {
            //Remove the security group from a subnet.
            await _azure.Networks.GetByResourceGroup(_commonResourceGroup, networkName)
                .Update().UpdateSubnet(subnetName).WithoutNetworkSecurityGroup().Parent().ApplyAsync();
        }


        public async Task NsgAllowInboundPort(string securityGroupName,
                                              string ruleName,
                                              int priority,
                                              string[] internalAddresses,
                                              int internalPort)
        {
            await _azure.NetworkSecurityGroups
                .GetByResourceGroup(_commonResourceGroup, securityGroupName) //can be changed to get by ID
                .Update()
                .DefineRule(ruleName)//Maybe "AllowOutgoing" + portvariable
                .AllowInbound()
                .FromAddresses(internalAddresses)
                .FromAnyPort()
                .ToAnyAddress()
                .ToPort(internalPort)
                .WithAnyProtocol()
                .WithPriority(priority)
                .Attach()
                .ApplyAsync();
        }


        public async Task NsgAllowOutboundPort(string securityGroupName,
                                               string ruleName,
                                               int priority,
                                               string[] externalAddresses,
                                               int externalPort)
        {
            await _azure.NetworkSecurityGroups
                .GetByResourceGroup(_commonResourceGroup, securityGroupName) //can be changed to get by ID
                .Update()
                .DefineRule(ruleName)
                .AllowOutbound()
                .FromAnyAddress()
                .FromAnyPort()
                .ToAddresses(externalAddresses)
                .ToPort(externalPort)
                .WithAnyProtocol()
                .WithPriority(priority)
                .Attach()
                .ApplyAsync();
        }

        public async Task NsgApplyBaseRules(INetworkSecurityGroup nsg)
        {
            await nsg.Update()
            .DefineRule("DenyInbound")
            .DenyInbound()
            .FromAnyAddress()
            .FromAnyPort()
            .ToAnyAddress()
            .ToAnyPort()
            .WithAnyProtocol()
            .WithPriority(4050)
            .Attach()

            .DefineRule("AllowVnetInBound2")
            .AllowInbound()
            .FromAddress("VirtualNetwork")
            .FromAnyPort()
            .ToAddress("VirtualNetwork")
            .ToAnyPort()
            .WithAnyProtocol()
            .WithPriority(4000)
            .Attach()

            .DefineRule("DenyOutbound")
            .DenyOutbound()
            .FromAnyAddress()
            .FromAnyPort()
            .ToAnyAddress()
            .ToAnyPort()
            .WithAnyProtocol()
            .WithPriority(4050)
            .Attach()
            
            .DefineRule("AllowVnetoutBound2")
            .AllowOutbound()
            .FromAddress("VirtualNetwork")
            .FromAnyPort()
            .ToAddress("VirtualNetwork")
            .ToAnyPort()
            .WithAnyProtocol()
            .WithPriority(4000)
            .Attach()
            .ApplyAsync();
        }

        public async Task<IEnumerable<string>> GetNSGNames()
        {
            var nsgs = await _azure.NetworkSecurityGroups.ListByResourceGroupAsync(_commonResourceGroup);
            return nsgs.Select(nsg => nsg.Name);
        }

        public async Task NsgAllowPort(string securityGroupName,
                                       string resourceGroupName,
                                       string ruleName,
                                       int priority,
                                       string[] internalAddresses,
                                       int internalPort,
                                       string[] externalAddresses,
                                       int externalPort)
        {
            await _azure.NetworkSecurityGroups
                .GetByResourceGroup(resourceGroupName, securityGroupName) //can be changed to get by ID
                .Update()
                .DefineRule(ruleName)
                .AllowInbound()
                .FromAddresses(internalAddresses)
                .FromPort(internalPort)
                .ToAddresses(externalAddresses)
                .ToPort(externalPort)
                .WithAnyProtocol()
                .WithPriority(priority)
                .Attach()
                .ApplyAsync();
        }


        //// Pod user/role management
        // Gives a user contributor to a resource group and network join on a network
        public async Task<string> AddUserToResourceGroup(string userId, string resourceGroupName) 
        {
            var resourceGroup = await _azure.ResourceGroups.GetByNameAsync(resourceGroupName);
            
            return _azure.AccessManagement.RoleAssignments
                .Define(Guid.NewGuid().ToString())
                .ForObjectId(userId)
                .WithBuiltInRole(BuiltInRole.Contributor)
                .WithResourceScope(resourceGroup)
                .CreateAsync().Result.Id;
        }

        public async Task<string> AddUserToNetwork(string userId, string networkName) 
        {
            var network = await _azure.Networks.GetByResourceGroupAsync(_commonResourceGroup, networkName);
            string joinNetworkRoleId = _azure.AccessManagement.RoleDefinitions
                .GetByScopeAndRoleNameAsync(network.Id, _joinNetworkRoleName).Result.Id;
            
            return _azure.AccessManagement.RoleAssignments
                .Define(Guid.NewGuid().ToString())
                .ForObjectId(userId)
                .WithRoleDefinition(joinNetworkRoleId)
                .WithResourceScope(network)
                .CreateAsync().Result.Id;
        }

    }
}
