﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Constants.CloudResource;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Dto.Azure;
using Sepes.Infrastructure.Dto.Sandbox;
using Sepes.Infrastructure.Dto.VirtualMachine;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Interface;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Query;
using Sepes.Infrastructure.Service.Azure.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class SandboxResourceService : ISandboxResourceService
    {
        readonly SepesDbContext _db;
        readonly IConfiguration _config;
        readonly ILogger<SandboxResourceService> _logger;
        readonly IMapper _mapper;
        readonly IUserService _userService;
        readonly IRequestIdService _requestIdService;
        readonly IAzureResourceGroupService _resourceGroupService;
        readonly ISandboxResourceOperationService _sandboxResourceOperationService;

        public SandboxResourceService(SepesDbContext db, IConfiguration config, IMapper mapper, ILogger<SandboxResourceService> logger, IUserService userService, IRequestIdService requestIdService, IAzureResourceGroupService resourceGroupService, ISandboxResourceOperationService sandboxResourceOperationService)
        {
            _db = db;
            _config = config;
            _logger = logger;
            _mapper = mapper;
            _userService = userService;
            _requestIdService = requestIdService;
            _resourceGroupService = resourceGroupService;
            _sandboxResourceOperationService = sandboxResourceOperationService ?? throw new ArgumentNullException(nameof(sandboxResourceOperationService));
        }

        public async Task CreateSandboxResourceGroup(SandboxResourceCreationAndSchedulingDto dto)
        {
            var resourceEntity = await AddInternal(dto.BatchId, dto.SandboxId, "not created", "not created", AzureResourceType.ResourceGroup, dto.Region.Name, dto.Tags);

            dto.ResourceGroup = MapEntityToDto(resourceEntity);

            var resourceGroupName = AzureResourceNameUtil.ResourceGroup(dto.StudyName, dto.SandboxName);

            var azureResourceGroup = await _resourceGroupService.Create(resourceGroupName, dto.Region, dto.Tags);
            ApplyPropertiesFromResourceGroup(azureResourceGroup, dto.ResourceGroup);

            _ = await UpdateResourceGroup(dto.ResourceGroup.Id.Value, dto.ResourceGroup);
            _ = await _sandboxResourceOperationService.UpdateStatusAsync(dto.ResourceGroup.Operations.FirstOrDefault().Id.Value, CloudResourceOperationState.DONE_SUCCESSFUL);
        }

        public async Task<SandboxResourceDto> CreateVmEntryAsync(StudyDto studyDto, SandboxDto sandboxDto, CreateVmUserInputDto userInput)
        {
            try
            {
                var sandboxId = sandboxDto.Id.Value;

                var virtualMachineName = AzureResourceNameUtil.VirtualMachine(studyDto.Name, sandboxDto.Name, userInput.Name);

                var tags = AzureResourceTagsFactory.CreateTags(_config, studyDto, sandboxDto);

                var region = RegionStringConverter.Convert(userInput.Region);

                var resourceGroupId = await SandboxResourceQueries.GetResourceGroupEntry(_db, sandboxId);

                //Make this dependent on bastion create operation to be completed, since bastion finishes last
                var dependsOn = await SandboxResourceQueries.GetCreateOperationIdForBastion(_db, sandboxId);

                var vmSettingsString = await CreateVmSettingsString(sandboxId, userInput);

                var resourceEntity = await AddInternal(Guid.NewGuid().ToString(),
                    sandboxId,
                    resourceGroupId.ResourceGroupId, resourceGroupId.ResourceGroupName, AzureResourceType.VirtualMachine, region.Name, tags, resourceName: virtualMachineName, false, dependentOn: dependsOn, configString: vmSettingsString);

                return MapEntityToDto(resourceEntity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create database resource entry for Virtual Machine for Sandbox {sandboxDto.Id.Value}. See inner Exception for details", ex);
            }
        }

        async Task<string> CreateVmSettingsString(int sandboxId, CreateVmUserInputDto userInput)
        {
            var vmSettings = _mapper.Map<VmSettingsDto>(userInput);

            var diagStorageResource = await SandboxResourceQueries.GetDiagStorageAccountEntry(_db, sandboxId);
            vmSettings.DiagnosticStorageAccountName = diagStorageResource.ResourceName;

            var networkResource = await SandboxResourceQueries.GetNetworkEntry(_db, sandboxId);
            vmSettings.NetworkName = networkResource.ResourceName;

            var networkSetting = SandboxResourceConfigStringSerializer.NetworkSettings(networkResource.ConfigString);
            vmSettings.SubnetName = networkSetting.SandboxSubnetName;

            return SandboxResourceConfigStringSerializer.Serialize(vmSettings);
        }

        public void ApplyPropertiesFromResourceGroup(AzureResourceGroupDto source, SandboxResourceDto target)
        {
            target.ResourceId = source.Id;
            target.ResourceName = source.Name;
            target.ResourceGroupId = source.Id;
            target.ResourceGroupName = source.Name;
            target.ProvisioningState = source.ProvisioningState;
            target.ResourceKey = source.Key;
        }

        public async Task<SandboxResourceDto> Create(SandboxResourceCreationAndSchedulingDto dto, string type, string resourceName, bool sandboxControlled = true, string configString = null)
        {
            var newResource = await AddInternal(dto.BatchId, dto.SandboxId, dto.ResourceGroupId, dto.ResourceGroupName, type, dto.Region.Name, dto.Tags, resourceName, sandboxControlled: sandboxControlled, configString: configString);

            return await GetByIdAsync(newResource.Id);
        }


        async Task<SandboxResource> AddInternal(string batchId, int sandboxId, string resourceGroupId, string resourceGroupName, string type, string region, Dictionary<string, string> tags, string resourceName = AzureResourceNameUtil.AZURE_RESOURCE_INITIAL_NAME, bool sandboxControlled = true, int dependentOn = 0, string configString = null)
        {
            var sandboxFromDb = await GetSandboxOrThrowAsync(sandboxId);

            var tagsString = AzureResourceTagsFactory.TagDictionaryToString(tags);

            var currentUser = await _userService.GetCurrentUserFromDbAsync();

            var newResource = new SandboxResource()
            {
                ResourceGroupId = resourceGroupId,
                ResourceGroupName = resourceGroupName,
                ResourceType = type,
                ResourceKey = "n/a",
                ResourceName = resourceName,
                ResourceId = "n/a",
                SandboxControlled = sandboxControlled,
                Region = region,
                Tags = tagsString,
                ConfigString = configString,

                Operations = new List<SandboxResourceOperation> {
                    new SandboxResourceOperation()
                    {
                    BatchId = batchId,
                    OperationType = CloudResourceOperationType.CREATE,
                    CreatedBySessionId = _requestIdService.GetRequestId(),
                    DependsOnOperationId = dependentOn != 0 ? dependentOn: default(int?),
                    }
                },
                CreatedBy = currentUser.UserName,
                Created = DateTime.UtcNow
            };

            sandboxFromDb.Resources.Add(newResource);

            await _db.SaveChangesAsync();

            return newResource;
        }

        public async Task<SandboxResourceDto> UpdateResourceGroup(int resourceId, SandboxResourceDto updated)
        {
            var currentUser = await _userService.GetCurrentUserFromDbAsync();

            var resource = await GetOrThrowAsync(resourceId);
            resource.ResourceGroupId = updated.ResourceId;
            resource.ResourceGroupName = updated.ResourceName;
            resource.ResourceId = updated.ResourceId;
            resource.ResourceKey = updated.ResourceKey;
            resource.ResourceName = updated.ResourceName;
            resource.LastKnownProvisioningState = updated.ProvisioningState;
            resource.Updated = DateTime.UtcNow;
            resource.UpdatedBy = currentUser.UserName;
            await _db.SaveChangesAsync();

            var retVal = await GetByIdAsync(resourceId);
            return retVal;
        }

        public async Task<SandboxResourceDto> Update(int resourceId, SandboxResourceDto updated)
        {
            var currentUser = await _userService.GetCurrentUserFromDbAsync();

            var resource = await GetOrThrowAsync(resourceId);
            resource.ResourceId = updated.ResourceId;
            resource.ResourceKey = updated.ResourceKey;
            resource.ResourceName = updated.ResourceName;
            resource.ResourceType = updated.ResourceType;
            resource.LastKnownProvisioningState = updated.ProvisioningState;
            resource.Updated = DateTime.UtcNow;
            resource.UpdatedBy = currentUser.UserName;
            await _db.SaveChangesAsync();

            var retVal = await GetByIdAsync(resourceId);
            return retVal;
        }

        public async Task<SandboxResourceDto> GetByIdAsync(int id)
        {
            var entityFromDb = await GetOrThrowAsync(id);

            var dto = MapEntityToDto(entityFromDb);

            return dto;
        }

        SandboxResourceDto MapEntityToDto(SandboxResource entity) => _mapper.Map<SandboxResourceDto>(entity);

        public async Task<SandboxResource> GetOrThrowAsync(int id)
        {
            var entityFromDb = await _db.SandboxResources.FirstOrDefaultAsync(s => s.Id == id);

            if (entityFromDb == null)
            {
                throw NotFoundException.CreateForEntity("AzureResource", id);
            }

            return entityFromDb;
        }

        public async Task<SandboxResourceDto> MarkAsDeletedByIdAsync(int id)
        {
            var resourceFromDb = await MarkAsDeletedByIdInternalAsync(id);
            return MapEntityToDto(resourceFromDb);
        }

        async Task<SandboxResource> MarkAsDeletedByIdInternalAsync(int id)
        {
            //WE DONT REALLY DELETE FROM THIS TABLE, WE "MARK AS DELETED" AND KEEP THE RECORDS FOR FUTURE REFERENCE

            var entityFromDb = await _db.SandboxResources.FirstOrDefaultAsync(s => s.Id == id);

            if (entityFromDb == null)
            {
                throw NotFoundException.CreateForEntity("AzureResource", id);
            }

            var user = _userService.GetCurrentUser();

            entityFromDb.DeletedBy = user.UserName;
            entityFromDb.Deleted = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return entityFromDb;
        }

        public async Task<List<SandboxResource>> GetActiveResources() => await _db.SandboxResources.Include(sr => sr.Sandbox)
                                                                                                   .ThenInclude(sb => sb.Study)
                                                                                                    .Include(sr => sr.Operations)
                                                                                                   .Where(sr => !sr.Deleted.HasValue)
                                                                                                   .ToListAsync();

        public async Task UpdateProvisioningState(int resourceId, string newProvisioningState)
        {
            var resource = await GetOrThrowAsync(resourceId);

            if (resource.LastKnownProvisioningState != newProvisioningState)
            {
                var currentUser = await _userService.GetCurrentUserFromDbAsync();

                resource.LastKnownProvisioningState = newProvisioningState;
                resource.Updated = DateTime.UtcNow;
                resource.UpdatedBy = currentUser.UserName;
                await _db.SaveChangesAsync();
            }

        }

        public async Task<SandboxResourceDto> UpdateMissingDetailsAfterCreation(int resourceId, string resourceIdInForeignSystem, string resourceNameInForeignSystem)
        {

            if (String.IsNullOrWhiteSpace(resourceIdInForeignSystem))
            {
                throw new ArgumentNullException("azureId", $"Provided empty foreign system resource id for resource {resourceId} ");
            }


            if (String.IsNullOrWhiteSpace(resourceNameInForeignSystem))
            {
                throw new ArgumentNullException("azureId", $"Provided empty foreign system resource name for resource {resourceId} ");
            }

            var resourceFromDb = await GetOrThrowAsync(resourceId);

            if (String.IsNullOrWhiteSpace(resourceFromDb.ResourceId) == false && resourceFromDb.ResourceId != AzureResourceNameUtil.AZURE_RESOURCE_INITIAL_NAME)
            {
                throw new Exception($"Resource {resourceId} allredy has a foreign system id. This should not have occured ");
            }

            resourceFromDb.ResourceId = resourceIdInForeignSystem;

            if (resourceFromDb.ResourceName != resourceNameInForeignSystem)
            {
                resourceFromDb.ResourceName = resourceNameInForeignSystem;
            }

            var currentUser = _userService.GetCurrentUser();

            resourceFromDb.Updated = DateTime.UtcNow;
            resourceFromDb.UpdatedBy = currentUser.UserName;

            await _db.SaveChangesAsync();

            return MapEntityToDto(resourceFromDb);

        }
        private async Task<Sandbox> GetSandboxOrThrowAsync(int sandboxId)
        {
            var sandboxFromDb = await _db.Sandboxes
                .Include(sb => sb.Resources)
                    .ThenInclude(r => r.Operations)
                .FirstOrDefaultAsync(sb => sb.Id == sandboxId);

            if (sandboxFromDb == null)
            {
                throw NotFoundException.CreateForEntity("Sandbox", sandboxId);
            }
            return sandboxFromDb;
        }

        public async Task<IEnumerable<SandboxResource>> GetDeletedResourcesAsync() => await _db.SandboxResources.Include(sr => sr.Operations).Where(sr => sr.Deleted.HasValue && sr.Deleted.Value.AddMinutes(10) < DateTime.UtcNow)
                                                                                                                .ToListAsync();


    }
}
