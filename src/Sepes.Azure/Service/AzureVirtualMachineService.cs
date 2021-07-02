using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute.Fluent.VirtualMachine.Definition;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Dto;
using Sepes.Azure.Service.Interface;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Provisioning;
using Sepes.Common.Dto.VirtualMachine;
using Sepes.Common.Util;

namespace Sepes.Azure.Service
{
    public class AzureVirtualMachineService : AzureVirtualMachineServiceBase, IAzureVirtualMachineService
    {
        readonly IMapper _mapper;
        readonly IAzureKeyVaultSecretService _azureKeyVaultSecretService;
        readonly IAzureNetworkSecurityGroupRuleService _azureNetworkSecurityGroupRuleService;


        public AzureVirtualMachineService(IConfiguration config, IMapper mapper, ILogger<AzureVirtualMachineService> logger,
            IAzureKeyVaultSecretService azureKeyVaultSecretService,
            IAzureNetworkSecurityGroupRuleService azureNetworkSecurityGroupRuleService
           )
            : base(config, logger)
        {
            _mapper = mapper;
            _azureKeyVaultSecretService = azureKeyVaultSecretService;
            _azureNetworkSecurityGroupRuleService = azureNetworkSecurityGroupRuleService;
        }

        public async Task<ResourceProvisioningResult> EnsureCreated(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Ensuring VM exists: {parameters.Name} in resource Group: {parameters.ResourceGroupName}");

            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(parameters.ConfigurationString);

            var virtualMachine = await GetInternalAsync(parameters.ResourceGroupName, parameters.Name, false);

            if (virtualMachine == null)
            {
                _logger.LogInformation($"VM {parameters.Name} did not exist in resource Group: {parameters.ResourceGroupName}, creating!");

                var passwordReference = vmSettings.Password;
                string password = await GetPasswordFromKeyVault(passwordReference);

                string vmSize = vmSettings.Size;

                virtualMachine = await CreateInternalAsync(GetRegionFromString(parameters.Region),
                    parameters.ResourceGroupName,
                    parameters.Name,
                    vmSettings.NetworkName, vmSettings.SubnetName,
                    vmSettings.Username, password,
                    vmSize, vmSettings.OperatingSystem, vmSettings.OperatingSystemCategory, parameters.Tags,
                    vmSettings.DiagnosticStorageAccountName, cancellationToken);

                await DeletePasswordFromKeyVault(passwordReference);

                if (vmSettings.DataDisks != null && vmSettings.DataDisks.Count > 0)
                {
                    foreach (var curDisk in vmSettings.DataDisks)
                    {
                        var sizeAsInt = Convert.ToInt32(curDisk);

                        if (sizeAsInt == 0)
                        {
                            throw new Exception($"Illegal data disk size: {curDisk}");
                        }

                        await ApplyVmDataDisksInternalAsync(parameters.ResourceGroupName, parameters.Name, sizeAsInt, parameters.Tags);
                    }
                }

                _logger.LogInformation($"Done creating Virtual Machine for sandbox with Id: {parameters.SandboxId}! Id: {virtualMachine.Id}");
            }
            else
            {
                //Validate data disks
                if (vmSettings.DataDisks != null && vmSettings.DataDisks.Count > 0)
                {
                    if (virtualMachine.DataDisks.Count != vmSettings.DataDisks.Count)
                    {
                        throw new Exception($"Data disk(s) not created properly. Expected count of {vmSettings.DataDisks}, saw {vmSettings.DataDisks.Count} on VM");
                    }
                }
            }

            var primaryNic = await _azure.NetworkInterfaces.GetByIdAsync(virtualMachine.PrimaryNetworkInterfaceId, cancellationToken);

            //Add tags to NIC
            await primaryNic.UpdateTags().WithTags(parameters.Tags).ApplyTagsAsync();

            await UpdateVmRules(parameters, vmSettings, primaryNic.PrimaryPrivateIP, cancellationToken);

            var result = CreateCRUDResult(virtualMachine);

            return result;
        }

        public async Task<ResourceProvisioningResult> Update(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Updating VM {parameters.Name}");

            var vm = await GetInternalAsync(parameters.ResourceGroupName, parameters.Name);
            var primaryNic = await _azure.NetworkInterfaces.GetByIdAsync(vm.PrimaryNetworkInterfaceId, cancellationToken);

            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(parameters.ConfigurationString);

            await UpdateVmRules(parameters, vmSettings, primaryNic.PrimaryPrivateIP, cancellationToken);

            var result = CreateCRUDResult(vm);

            return result;
        }

        async Task UpdateVmRules(ResourceProvisioningParameters parameters, VmSettingsDto vmSettings, string privateIp, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Setting desired VM rules for {parameters.Name}");

            var existingRules = await _azureNetworkSecurityGroupRuleService.GetNsgRulesContainingName(parameters.ResourceGroupName, parameters.NetworkSecurityGroupName, $"{AzureResourceNameUtil.NSG_RULE_FOR_VM_PREFIX}{parameters.DatabaseId}", cancellationToken);
            var existingRulesThatStillExists = new HashSet<string>();

            if (vmSettings.Rules == null)
            {
                throw new Exception($"No rules exists for VM {parameters.Name}");
            }
            else
            {
                foreach (var curRule in vmSettings.Rules)
                {
                    try
                    {
                        var ruleMapped = _mapper.Map<NsgRuleDto>(curRule);

                        if (curRule.Direction == RuleDirection.Inbound)
                        {
                            ruleMapped.SourceAddress = curRule.Ip;
                            ruleMapped.SourcePort = curRule.Port;
                            ruleMapped.DestinationAddress = privateIp;
                            ruleMapped.DestinationPort = curRule.Port;

                            //get existing rule and use that name
                            if (existingRules.TryGetValue(curRule.Name, out NsgRuleDto existingRule))
                            {
                                existingRulesThatStillExists.Add(curRule.Name);
                                ruleMapped.Priority = existingRule.Priority;
                                await _azureNetworkSecurityGroupRuleService.UpdateInboundRule(parameters.ResourceGroupName, parameters.NetworkSecurityGroupName, ruleMapped, cancellationToken);
                            }
                            else
                            {
                                ruleMapped.Priority = await FindNextPriority(parameters, existingRules, AzureVmConstants.MIN_RULE_PRIORITY, 10, AzureVmConstants.MAX_RULE_PRIORITY, curRule.Direction.ToString());
                                await _azureNetworkSecurityGroupRuleService.AddInboundRule(parameters.ResourceGroupName, parameters.NetworkSecurityGroupName, ruleMapped, cancellationToken);
                            }

                        }
                        else
                        {
                            ruleMapped.SourceAddress = privateIp;
                            ruleMapped.SourcePort = curRule.Port;

                            if (ruleMapped.Name.Contains(AzureVmConstants.RulePresets.OPEN_CLOSE_INTERNET))
                            {
                                ruleMapped.DestinationAddress = "*";
                                ruleMapped.DestinationPort = 0;
                            }
                            else
                            {
                                ruleMapped.DestinationAddress = curRule.Ip;
                                ruleMapped.DestinationPort = curRule.Port;
                            }

                            if (existingRules.TryGetValue(curRule.Name, out NsgRuleDto existingRule))
                            {
                                existingRulesThatStillExists.Add(curRule.Name);
                                ruleMapped.Priority = existingRule.Priority; //Ensure same priority is re-used
                                await _azureNetworkSecurityGroupRuleService.UpdateOutboundRule(parameters.ResourceGroupName, parameters.NetworkSecurityGroupName, ruleMapped, cancellationToken);
                            }
                            else
                            {

                                ruleMapped.Priority = await FindNextPriority(parameters, existingRules, AzureVmConstants.MIN_RULE_PRIORITY, 10, AzureVmConstants.MAX_RULE_PRIORITY, curRule.Direction.ToString());
                                await _azureNetworkSecurityGroupRuleService.AddOutboundRule(parameters.ResourceGroupName, parameters.NetworkSecurityGroupName, ruleMapped, cancellationToken);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Unable to create rule {curRule.Name} for VM {parameters.Name}", ex);
                    }
                }
            }

            if (existingRules != null && existingRules.Count > 0)
            {
                foreach (var curExistingKvp in existingRules)
                {
                    if (!existingRulesThatStillExists.Contains(curExistingKvp.Key))
                    {
                        await _azureNetworkSecurityGroupRuleService.DeleteRule(parameters.ResourceGroupName, parameters.NetworkSecurityGroupName, curExistingKvp.Key, cancellationToken);
                    }
                }
            }

            _logger.LogInformation($"Done setting desired VM rules for {parameters.Name}");
        }

        async Task<int> FindNextPriority(ResourceProvisioningParameters parameters, Dictionary<string, NsgRuleDto> existingRulesForVm, int startingAt, int increaseBy, int highestAllowed, string direction, CancellationToken cancellationToken = default)
        {
            var existingRuleForSameVmWithHighestPriority = existingRulesForVm.Values.Where(r => r.Direction == direction).OrderByDescending(r => r.Priority).FirstOrDefault();

            if (existingRuleForSameVmWithHighestPriority != null)
            {
                startingAt = existingRuleForSameVmWithHighestPriority.Priority += increaseBy;
            }

            return await FindNextPriority(parameters, startingAt, increaseBy, highestAllowed, direction, cancellationToken);

        }

        async Task<int> FindNextPriority(ResourceProvisioningParameters parameters, int startingAt, int increaseBy, int highestAllowed, string direction, CancellationToken cancellationToken = default)
        {
            var allExistingRulesInNsg = await _azureNetworkSecurityGroupRuleService.GetNsgRulesForDirection(parameters.ResourceGroupName, parameters.NetworkSecurityGroupName, direction, cancellationToken);

            var relevantExistingRulesInNsg = allExistingRulesInNsg.Where(r => r.Value.Priority >= startingAt && r.Value.Priority <= highestAllowed).OrderBy(r => r.Value.Priority);

            if (relevantExistingRulesInNsg.Count() == 0)
            {
                return startingAt;
            }

            var curPriority = startingAt;

            while (curPriority <= highestAllowed)
            {
                bool collision = false;

                foreach (var curExisting in relevantExistingRulesInNsg)
                {
                    if (curExisting.Value == null)
                    {
                        break;
                    }
                    if (curPriority > curExisting.Value.Priority)
                    {
                        continue;
                    }
                    else if ((curExisting.Value.Priority - curPriority) < 10)
                    {
                        collision = true;
                        break;
                    }


                }

                if (collision)
                {
                    curPriority += increaseBy;
                }
                else
                {
                    return curPriority;
                }
            }


            throw new Exception($"Unable to determine next priority for vm {parameters.Name}. Stopped at {highestAllowed}");

        }

        public async Task<ResourceProvisioningResult> GetSharedVariables(ResourceProvisioningParameters parameters)
        {
            var vm = await GetInternalAsync(parameters.ResourceGroupName, parameters.Name);
            var result = CreateCRUDResult(vm);
            return result;
        }

        async Task<string> GetPasswordFromKeyVault(string passwordId)
        {
            try
            {
                return await _azureKeyVaultSecretService.GetKeyVaultSecretValue(ConfigConstants.AZURE_VM_TEMP_PASSWORD_KEY_VAULT, passwordId);
            }
            catch (Exception ex)
            {

                throw new Exception($"VM Creation failed. Unable to get VM password from Key Vault. See inner exception for details.", ex);
            }

        }

        async Task DeletePasswordFromKeyVault(string passwordId)
        {
            try
            {
                await _azureKeyVaultSecretService.DeleteKeyVaultSecretValue(ConfigConstants.AZURE_VM_TEMP_PASSWORD_KEY_VAULT, passwordId, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"VM Creation: Unable to delete VM password from Key Vault after use. Contiuning VM creation, but password must be deleted manually");
            }
        }

        async Task<IVirtualMachine> CreateInternalAsync(Region region, string resourceGroupName, string vmName, string primaryNetworkName, string subnetName, string userName, string password, string vmSize, string osName, string osCategory, IDictionary<string, string> tags, string diagStorageAccountName, CancellationToken cancellationToken = default)
        {
            IVirtualMachine vm;

            // Get diagnostic storage account reference for boot diagnostics
            var diagStorage = _azure.StorageAccounts.GetByResourceGroup(resourceGroupName, diagStorageAccountName);

            AzureResourceUtil.ThrowIfResourceIsNull(diagStorage, AzureResourceType.StorageAccount, diagStorageAccountName, "Create VM failed");

            var network = await _azure.Networks.GetByResourceGroupAsync(resourceGroupName, primaryNetworkName);

            AzureResourceUtil.ThrowIfResourceIsNull(network, AzureResourceType.VirtualNetwork, primaryNetworkName, "Create VM failed");

            var publicIpName = AzureResourceNameUtil.VirtualMachinePublicIp(vmName);

            var pip = await _azure.PublicIPAddresses.Define(publicIpName)
             .WithRegion(region)
             .WithExistingResourceGroup(resourceGroupName)
             .WithStaticIP()
             .WithSku(PublicIPSkuType.Standard)
             .WithTags(tags)
             .CreateAsync(cancellationToken);

            var vmCreatable = _azure.VirtualMachines.Define(vmName)
                                    .WithRegion(region)
                                    .WithExistingResourceGroup(resourceGroupName)
                                    .WithExistingPrimaryNetwork(network)
                                    .WithSubnet(subnetName)
                                    .WithPrimaryPrivateIPAddressDynamic()
                                    .WithExistingPrimaryPublicIPAddress(pip);


            IWithCreate vmWithOS;

            if (osCategory.ToLower().Equals("windows"))
            {
                vmWithOS = CreateWindowsVm(vmCreatable, osName, userName, password);
            }
            else if (osCategory.ToLower().Equals("linux"))
            {
                vmWithOS = CreateLinuxVm(vmCreatable, osName, userName, password);
            }
            else
            {
                throw new ArgumentException($"Argument 'osCategory' needs to be either 'windows' or 'linux'. Current value: {osCategory}");
            }

            var vmWithSize = vmWithOS.WithSize(vmSize);

            vm = await vmWithSize
                .WithBootDiagnostics(diagStorage)
                .WithTags(tags)
                .CreateAsync(cancellationToken);

            return vm;

        }

        private IWithManagedCreate CreateWindowsVm(IWithProximityPlacementGroup vmCreatable, string distro, string userName, string password)
        {
            IWithWindowsAdminUsernameManagedOrUnmanaged withOS;

            switch (distro.ToLower())
            {
                case "win2019datacenter":
                    withOS = vmCreatable.WithLatestWindowsImage(AzureVmOperatingSystemConstants.Windows.Server2019DataCenter.Publisher, AzureVmOperatingSystemConstants.Windows.Server2019DataCenter.Offer, AzureVmOperatingSystemConstants.Windows.Server2019DataCenter.Sku);
                    break;
                case "win2019datacentercore":
                    withOS = vmCreatable.WithLatestWindowsImage(AzureVmOperatingSystemConstants.Windows.Server2019DataCenterCore.Publisher, AzureVmOperatingSystemConstants.Windows.Server2019DataCenterCore.Offer, AzureVmOperatingSystemConstants.Windows.Server2019DataCenterCore.Sku);
                    break;
                case "win2016datacenter":
                    withOS = vmCreatable.WithLatestWindowsImage(AzureVmOperatingSystemConstants.Windows.Server2016DataCenter.Publisher, AzureVmOperatingSystemConstants.Windows.Server2016DataCenter.Offer, AzureVmOperatingSystemConstants.Windows.Server2016DataCenter.Sku);
                    break;
                case "win2016datacentercore":
                    withOS = vmCreatable.WithLatestWindowsImage(AzureVmOperatingSystemConstants.Windows.Server2016DataCenterCore.Publisher, AzureVmOperatingSystemConstants.Windows.Server2016DataCenterCore.Offer, AzureVmOperatingSystemConstants.Windows.Server2016DataCenterCore.Sku);
                    break;
                default:
                    _logger.LogInformation("Could not match distro argument. Default will be chosen: Windows Server 2019");
                    withOS = vmCreatable.WithLatestWindowsImage(AzureVmOperatingSystemConstants.Windows.Server2019DataCenter.Publisher, AzureVmOperatingSystemConstants.Windows.Server2019DataCenter.Offer, AzureVmOperatingSystemConstants.Windows.Server2019DataCenter.Sku);
                    break;
            }
            var vm = withOS
                .WithAdminUsername(userName)
                .WithAdminPassword(password)
                .WithOSDiskStorageAccountType(StorageAccountTypes.StandardSSDLRS);
            return vm;
        }

        private IWithManagedCreate CreateLinuxVm(IWithProximityPlacementGroup vmCreatable, string distro, string userName, string password)
        {
            IWithLinuxRootUsernameManagedOrUnmanaged withOS;

            switch (distro.ToLower())
            {
                case "ubuntults":
                    withOS = vmCreatable.WithLatestLinuxImage(AzureVmOperatingSystemConstants.Linux.UbuntuServer1804LTS.Publisher, AzureVmOperatingSystemConstants.Linux.UbuntuServer1804LTS.Offer, AzureVmOperatingSystemConstants.Linux.UbuntuServer1804LTS.Sku);
                    break;
                case "ubuntu16lts":
                    withOS = vmCreatable.WithPopularLinuxImage(KnownLinuxVirtualMachineImage.UbuntuServer16_04_Lts);
                    break;
                case "rhel":
                    withOS = vmCreatable.WithLatestLinuxImage(AzureVmOperatingSystemConstants.Linux.RedHat7LVM.Publisher, AzureVmOperatingSystemConstants.Linux.RedHat7LVM.Offer, AzureVmOperatingSystemConstants.Linux.RedHat7LVM.Sku);
                    break;
                case "debian":
                    withOS = vmCreatable.WithLatestLinuxImage(AzureVmOperatingSystemConstants.Linux.Debian10.Publisher, AzureVmOperatingSystemConstants.Linux.Debian10.Offer, AzureVmOperatingSystemConstants.Linux.Debian10.Sku);
                    break;
                case "centos":
                    withOS = vmCreatable.WithLatestLinuxImage(AzureVmOperatingSystemConstants.Linux.CentOS75.Publisher, AzureVmOperatingSystemConstants.Linux.CentOS75.Offer, AzureVmOperatingSystemConstants.Linux.CentOS75.Sku);
                    break;
                default:
                    _logger.LogInformation("Could not match distro argument. Default will be chosen: Ubuntu 18.04-LTS");
                    withOS = vmCreatable.WithLatestLinuxImage(AzureVmOperatingSystemConstants.Linux.UbuntuServer1804LTS.Publisher, AzureVmOperatingSystemConstants.Linux.UbuntuServer1804LTS.Offer, AzureVmOperatingSystemConstants.Linux.UbuntuServer1804LTS.Sku);
                    break;
            }
            var vm = withOS
                .WithRootUsername(userName)
                .WithRootPassword(password)
              .WithOSDiskStorageAccountType(StorageAccountTypes.StandardSSDLRS);
            return vm;
        }

        async Task ApplyVmDataDisksInternalAsync(string resourceGroupName, string virtualMachineName, int sizeInGB, Dictionary<string, string> tags)
        {
            var vm = await GetInternalAsync(resourceGroupName, virtualMachineName);

            //Ensure resource is is managed by this instance
            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, vm.Tags);

            var newDataDisk = await _azure.Disks
                .Define($"hdd-{virtualMachineName}-data-{Guid.NewGuid().ToString().Substring(0, 5)}")
                .WithRegion(vm.RegionName)

                .WithExistingResourceGroup(resourceGroupName)
                .WithData()
                .WithSizeInGB(sizeInGB)
                .WithSku(DiskSkuTypes.SStandardSSDLRS)
                .WithTags(tags)
                .CreateAsync();

            var diskUpdateRequest = vm.Update().WithExistingDataDisk(newDataDisk);

            if (sizeInGB > 4095)
            {
                diskUpdateRequest = diskUpdateRequest.WithDataDiskDefaultCachingType(CachingTypes.None);
            }

            await diskUpdateRequest.ApplyAsync();
        }

        public async Task<ResourceProvisioningResult> EnsureDeleted(ResourceProvisioningParameters parameters)
        {
            await DeleteInternalAsync(parameters.ResourceGroupName, parameters.Name, parameters.NetworkSecurityGroupName, parameters.ConfigurationString);
            var provisioningState = await GetProvisioningState(parameters.ResourceGroupName, parameters.Name);

            return ResourceProvisioningResultUtil.CreateFromProvisioningState(provisioningState);
        }

        async Task DeleteInternalAsync(string resourceGroupName, string virtualMachineName, string networkSecurityGroupName, string configString)
        {
            var vm = await GetInternalAsync(resourceGroupName, virtualMachineName, false);

            if (vm == null)
            {
                _logger.LogWarning($"Virtual Machine {virtualMachineName} appears to be deleted allready");
                return;
            }

            //Ensure resource is is managed by this instance
            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, vm.Tags);

            await _azure.VirtualMachines.DeleteByResourceGroupAsync(resourceGroupName, virtualMachineName);

            //Delete all the disks
            await DeleteDiskById(vm.OSDiskId);

            foreach (var curNic in vm.NetworkInterfaceIds)
            {
                await DeleteNic(curNic);
            }

            foreach (var curDiskKvp in vm.DataDisks)
            {
                await DeleteDiskById(curDiskKvp.Value.Id);
            }

            //Delete VM rules
            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(configString);

            foreach (var curRule in vmSettings.Rules)
            {
                try
                {
                    await _azureNetworkSecurityGroupRuleService.DeleteRule(resourceGroupName, networkSecurityGroupName, curRule.Name);
                }
                catch (Exception)
                {
                    _logger.LogWarning($"Delete VM: Failed to delete NSG rule {curRule.Name} for vm {virtualMachineName}. Assuming it has allready been deleted");
                }
            }
        }

        public async Task DeleteNic(string id)
        {
            await _azure.NetworkInterfaces.DeleteByIdAsync(id);
        }

        public async Task DeleteDiskById(string id)
        {
            await _azure.Disks.DeleteByIdAsync(id);
        }

        public async Task<IDictionary<string, string>> GetTagsAsync(string resourceGroupName, string resourceName)
        {
            var resource = await GetInternalAsync(resourceGroupName, resourceName);
            return TagUtils.TagReadOnlyDictionaryToDictionary(resource.Tags);
        }

        public async Task UpdateTagAsync(string resourceGroupName, string resourceName, KeyValuePair<string, string> tag, CancellationToken cancellationToken = default)
        {
            var resource = await GetInternalAsync(resourceGroupName, resourceName);

            //Ensure resource is is managed by this instance
            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.Update().WithoutTag(tag.Key).ApplyAsync(cancellationToken);
            _ = await resource.Update().WithTag(tag.Key, tag.Value).ApplyAsync(cancellationToken);
        }

        public async Task SetTagsAsync(string resourceGroupName, string resourceName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var resource = await GetInternalAsync(resourceGroupName, resourceName);

            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.Update().WithTags(tags).ApplyAsync(cancellationToken);

            await UpdateRelatedResourcesTags(resourceGroupName, resource, tags, cancellationToken);
        }

        async Task UpdateRelatedResourcesTags(string resourceGroupName, IVirtualMachine virtualMachine, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            //Update NIC tags
            var primaryNic = await _azure.NetworkInterfaces.GetByIdAsync(virtualMachine.PrimaryNetworkInterfaceId, cancellationToken);
            await primaryNic.UpdateTags().WithTags(tags).ApplyTagsAsync(cancellationToken);

            //Update public IP tags
            if (primaryNic.PrimaryIPConfiguration != null)
            {
                var pip = await _azure.PublicIPAddresses.GetByIdAsync(primaryNic.PrimaryIPConfiguration.PublicIPAddressId, cancellationToken);

                if (pip != null)
                {
                    await _azure.PublicIPAddresses.Inner.UpdateTagsWithHttpMessagesAsync(resourceGroupName, pip.Name, tags, cancellationToken: cancellationToken);
                }
            }

            //Update disk tags
            await UpdateDiskTags(virtualMachine.OSDiskId, tags);

            foreach(var curDataDisk in virtualMachine.DataDisks)
            {
                await UpdateDiskTags(curDataDisk.Value.Id, tags);
            }
        }

        async Task UpdateDiskTags(string diskId, Dictionary<string, string> tags)
        {
            var disk = await _azure.Disks.GetByIdAsync(diskId);

            await disk.Update().WithTags(tags).ApplyAsync();
        }

        ResourceProvisioningResult CreateCRUDResult(IVirtualMachine vm)
        {
            var crudResult = ResourceProvisioningResultUtil.CreateFromIResource(vm);
            crudResult.CurrentProvisioningState = vm.Inner.ProvisioningState.ToString();
            return crudResult;
        }
    }
}
