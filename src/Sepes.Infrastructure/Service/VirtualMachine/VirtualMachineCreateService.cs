using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Service.Interface;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Sandbox;
using Sepes.Common.Dto.VirtualMachine;
using Sepes.Common.Util;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Query;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class VirtualMachineCreateService : VirtualMachineServiceBase, IVirtualMachineCreateService
    {
        readonly ISandboxModelService _sandboxModelService;
        readonly ICloudResourceCreateService _cloudResourceCreateService;
        readonly ICloudResourceUpdateService _cloudResourceUpdateService;
        readonly ICloudResourceDeleteService _cloudResourceDeleteService;

        readonly IProvisioningQueueService _provisioningQueueService;
        readonly IAzureKeyVaultSecretService _azureKeyVaultSecretService;
        readonly IVirtualMachineOperatingSystemService _virtualMachineOperatingSystemService;     


        public VirtualMachineCreateService(
            IConfiguration config,
            SepesDbContext db,
            ILogger<VirtualMachineCreateService> logger,
            IMapper mapper,
            IUserService userService,

            ISandboxModelService sandboxModelService,
            ICloudResourceCreateService cloudResourceCreateService,
            ICloudResourceReadService cloudResourceReadService,
            ICloudResourceUpdateService cloudResourceUpdateService,
            ICloudResourceDeleteService cloudResourceDeleteService,
            IProvisioningQueueService provisioningQueueService,
            IAzureKeyVaultSecretService azureKeyVaultSecretService,
            IVirtualMachineOperatingSystemService virtualMachineOperatingSystemService

          )
           : base(config, db, logger, mapper, userService, cloudResourceReadService)
        {
            _sandboxModelService = sandboxModelService;
            _cloudResourceCreateService = cloudResourceCreateService;
            _cloudResourceUpdateService = cloudResourceUpdateService;
            _cloudResourceDeleteService = cloudResourceDeleteService;
            _provisioningQueueService = provisioningQueueService;
            _azureKeyVaultSecretService = azureKeyVaultSecretService;
            _virtualMachineOperatingSystemService = virtualMachineOperatingSystemService;
        }

        public async Task<VmDto> CreateAsync(int sandboxId, VirtualMachineCreateDto userInput)
        {
            CloudResource vmResourceEntry = null;

            try
            {
                ValidateVmPasswordOrThrow(userInput.Password);

                GenericNameValidation.ValidateName(userInput.Name);

                _logger.LogInformation($"Creating Virtual Machine for sandbox: {sandboxId}");

                var sandbox = await _sandboxModelService.GetByIdForResourceCreationAsync(sandboxId, UserOperation.Study_Crud_Sandbox);

                var virtualMachineName = AzureResourceNameUtil.VirtualMachine(sandbox.Study.Name, sandbox.Name, userInput.Name);

                await _cloudResourceCreateService.ValidateThatNameDoesNotExistThrowIfInvalid(virtualMachineName);

                var tags = ResourceTagFactory.SandboxResourceTags(_config, sandbox.Study, sandbox);

                var region = RegionStringConverter.Convert(sandbox.Region);

                userInput.DataDisks = await TranslateDiskSizes(sandbox.Region, userInput.DataDisks);

                var resourceGroup = await CloudResourceQueries.GetResourceGroupEntry(_db, sandboxId);

                //Make this dependent on bastion create operation to be completed, since bastion finishes last
                var dependsOn = await CloudResourceQueries.GetCreateOperationIdForBastion(_db, sandboxId);

                vmResourceEntry = await _cloudResourceCreateService.CreateVmEntryAsync(sandboxId, resourceGroup, region.Name, tags, virtualMachineName, dependsOn, null);

                //Create vm settings and immeately attach to resource entry
                var vmSettingsString = await CreateVmSettingsString(sandbox.Region, vmResourceEntry.Id, sandbox.Study.Id, sandboxId, userInput);
                vmResourceEntry.ConfigString = vmSettingsString;
                await _cloudResourceUpdateService.Update(vmResourceEntry.Id, vmResourceEntry);

                var queueParentItem = new ProvisioningQueueParentDto
                {
                    Description = $"Create VM for Sandbox: {sandboxId}"
                };

                queueParentItem.Children.Add(new ProvisioningQueueChildDto() { ResourceOperationId = vmResourceEntry.Operations.FirstOrDefault().Id });

                await _provisioningQueueService.SendMessageAsync(queueParentItem);

                var dtoMappedFromResource = _mapper.Map<VmDto>(vmResourceEntry);

                return dtoMappedFromResource;
            }
            catch (Exception ex)
            {
                try
                {
                    //Delete resource if created
                    if (vmResourceEntry != null)
                    {
                        await _cloudResourceDeleteService.HardDeleteAsync(vmResourceEntry.Id);
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, $"Failed to roll back VM creation for sandbox {sandboxId}");
                }

                throw new Exception($"Failed to create VM: {ex.Message}", ex);
            }
        }

        public void ValidateVmPasswordOrThrow(string password)
        {
            var errorString = "";
            //Atleast one upper case
            var upper = new Regex(@"(?=.*[A-Z])");
            //Atleast one number
            var number = new Regex(@".*[0-9].*");
            //Atleast one special character
            var special = new Regex(@"(?=.*[!@#$%^&*])");
            //Between 12-123 long
            var limit = new Regex(@"(?=.{12,123})");

            if (!upper.IsMatch(password))
            {
                errorString += "Missing one uppercase character. ";
            }
            if (!number.IsMatch(password))
            {
                errorString += "Missing one number. ";
            }
            if (!special.IsMatch(password))
            {
                errorString += "Missing one special character. ";
            }
            if (!limit.IsMatch(password))
            {
                errorString += "Outside the limit (12-123). ";
            }

            if (!String.IsNullOrWhiteSpace(errorString))
            {
                throw new Exception($"Password is missing following requirements: {errorString}");
            }
        }

        async Task<string> CreateVmSettingsString(string region, int vmId, int studyId, int sandboxId, VirtualMachineCreateDto userInput)
        {
            var vmSettings = _mapper.Map<VmSettingsDto>(userInput);

            var availableOs = await _virtualMachineOperatingSystemService.AvailableOperatingSystems(region);
            vmSettings.OperatingSystemCategory = AzureVmUtil.GetOsCategory(availableOs, vmSettings.OperatingSystem);

            vmSettings.Password = await StoreNewVmPasswordAsKeyVaultSecretAndReturnReference(studyId, sandboxId, vmSettings.Password);

            var diagStorageResource = await CloudResourceQueries.GetDiagStorageAccountEntry(_db, sandboxId);
            vmSettings.DiagnosticStorageAccountName = diagStorageResource.ResourceName;

            var networkResource = await CloudResourceQueries.GetNetworkEntry(_db, sandboxId);
            vmSettings.NetworkName = networkResource.ResourceName;

            var networkSetting = CloudResourceConfigStringSerializer.NetworkSettings(networkResource.ConfigString);
            vmSettings.SubnetName = networkSetting.SandboxSubnetName;

            vmSettings.Rules = VmRuleUtils.CreateInitialVmRules(vmId);
            return CloudResourceConfigStringSerializer.Serialize(vmSettings);
        }

        async Task<string> StoreNewVmPasswordAsKeyVaultSecretAndReturnReference(int studyId, int sandboxId, string password)
        {
            try
            {
                var keyVaultSecretName = $"newvmpassword-{studyId}-{sandboxId}-{Guid.NewGuid().ToString().Replace("-", "")}";

                await _azureKeyVaultSecretService.AddKeyVaultSecret(ConfigConstants.AZURE_VM_TEMP_PASSWORD_KEY_VAULT, keyVaultSecretName, password);

                return keyVaultSecretName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to store VM password in Key Vault. See inner exception for details.", ex);
            }
        }

        async Task<List<string>> TranslateDiskSizes(string region, List<string> dataDisksFromClient)
        {
            //Fix disks
            var disksFromDbForGivenRegion = await _db.RegionDiskSize.Where(rds => rds.RegionKey == region).Select(rds => rds.DiskSize).ToDictionaryAsync(ds => ds.Key, ds => ds);

            if (disksFromDbForGivenRegion.Count == 0)
            {
                throw new Exception($"No data disk items found in DB");
            }

            var result = new List<string>();

            foreach (var curDataDisk in dataDisksFromClient)
            {
                if (disksFromDbForGivenRegion.TryGetValue(curDataDisk, out DiskSize diskSize))
                {
                    result.Add(Convert.ToString(diskSize.Size));
                }
                else
                {
                    throw new Exception($"Unknown data disk size specification: {curDataDisk}");
                }
            }

            return result;
        }
    }
}
