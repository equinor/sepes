﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Common.Constants.CloudResource;
using Sepes.Common.Dto;
using Sepes.Common.Exceptions;
using Sepes.Common.Util.Provisioning;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class CloudResourceDeleteService : CloudResourceServiceBase, ICloudResourceDeleteService
    {
        readonly IUserService _userService;
        readonly ICloudResourceOperationReadService _cloudResourceOperationReadService;
        readonly ICloudResourceOperationCreateService _cloudResourceOperationCreateService;
        readonly ICloudResourceOperationUpdateService _cloudResourceOperationUpdateService;

        public CloudResourceDeleteService(SepesDbContext db,
            IConfiguration config,
            IMapper mapper,
            ILogger<CloudResourceDeleteService> logger,
            IUserService userService,
            IStudyPermissionService studyPermissionService,
            ICloudResourceOperationReadService cloudResourceOperationService,
            ICloudResourceOperationCreateService cloudResourceOperationCreateService,
            ICloudResourceOperationUpdateService cloudResourceOperationUpdateService
            )
         : base(db, config, mapper, logger, studyPermissionService)
        {
            _userService = userService;
            _cloudResourceOperationReadService = cloudResourceOperationService;
            _cloudResourceOperationCreateService = cloudResourceOperationCreateService;
            _cloudResourceOperationUpdateService = cloudResourceOperationUpdateService;
        }

        public async Task<CloudResourceOperationDto> MarkAsDeletedWithDeleteOperationAsync(int resourceId, UserOperation operation)
        {
            var resourceFromDb = await GetInternalAsync(resourceId, operation, throwIfNotFound: true);

            var deletePrefixForLogMessages = $"Marking resource {resourceId} ({resourceFromDb.ResourceType}) for deletion";

            _logger.LogInformation($"{deletePrefixForLogMessages}: Aborting all other operations for Resource");

            await _cloudResourceOperationUpdateService.AbortAllUnfinishedCreateOrUpdateOperationsAsync(resourceId);

            var currentUser = await _userService.GetCurrentUserAsync();

            _logger.LogInformation($"{deletePrefixForLogMessages}: Marking db entry as deleted");

            MarkAsDeletedInternal(resourceFromDb, currentUser.UserName);

            var deleteOperation = await EnsureExistsDeleteOperationInternalAsync(deletePrefixForLogMessages, resourceFromDb);

            await _db.SaveChangesAsync();

            _logger.LogInformation($"{deletePrefixForLogMessages}: Done!");

            return _mapper.Map<CloudResourceOperationDto>(deleteOperation);
        }

        public async Task<CloudResourceDto> MarkAsDeletedAsync(int resourceId)
        {
            var resourceFromDb = await GetInternalWithoutAccessCheckAsync(resourceId, throwIfNotFound: true);

            var user = await _userService.GetCurrentUserAsync();

            var deletePrefixForLogMessages = $"Marking resource {resourceId} ({resourceFromDb.ResourceType}) for deletion";

            _logger.LogInformation($"{deletePrefixForLogMessages}: Aborting all other operations for Resource");

            await _cloudResourceOperationUpdateService.AbortAllUnfinishedCreateOrUpdateOperationsAsync(resourceId);

            _logger.LogInformation($"{deletePrefixForLogMessages}: Marking db entry as deleted");

            MarkAsDeletedInternal(resourceFromDb, user.UserName);

            await _db.SaveChangesAsync();

            _logger.LogInformation($"{deletePrefixForLogMessages}: Done!");

            return _mapper.Map<CloudResourceDto>(resourceFromDb);
        }

        async Task<CloudResourceOperation> EnsureExistsDeleteOperationInternalAsync(string deleteDescription, CloudResource resource)
        {
            _logger.LogInformation($"{deleteDescription}: Ensuring delete operation exist");

            var deleteOperation = await _cloudResourceOperationReadService.GetUnfinishedDeleteOperation(resource.Id);

            if (deleteOperation == null)
            {
                _logger.LogInformation($"{deleteDescription}: Creating delete operation");

                deleteOperation = await _cloudResourceOperationCreateService.CreateDeleteOperationAsync(resource.Id,
                    ResourceOperationDescriptionUtils.CreateDescriptionForResourceOperation(
                        resource.ResourceType,
                        CloudResourceOperationType.DELETE,
                        resource.Id,
                        sandboxId: resource.SandboxId.Value));

            }
            else
            {
                _logger.LogInformation($"{deleteDescription}: Existing delete operation found, re-queueing that");
                await _cloudResourceOperationUpdateService.ReInitiateAsync(resource.Id);

            }

            return deleteOperation;
        }

        async Task<CloudResource> MarkAsDeletedByIdInternalAsync(int id)
        {
            var resourceEntity = await _db.CloudResources.FirstOrDefaultAsync(s => s.Id == id);

            if (resourceEntity == null)
            {
                throw NotFoundException.CreateForEntity("SandboxResource", id);
            }

            var user = await _userService.GetCurrentUserAsync();

            MarkAsDeletedInternal(resourceEntity, user.UserName);

            await _db.SaveChangesAsync();

            return resourceEntity;
        }

        void MarkAsDeletedInternal(CloudResource resource, string deletedBy)
        {
            SoftDeleteUtil.MarkAsDeleted(resource, deletedBy);
        }

        public async Task HardDeleteAsync(int resourceId)
        {
            var resourceFromDb = await GetInternalWithoutAccessCheckAsync(resourceId, throwIfNotFound: true);

            if (resourceFromDb != null)
            {
                foreach (var curOperation in resourceFromDb.Operations)
                {
                    _db.CloudResourceOperations.Remove(curOperation);
                }

                _db.CloudResources.Remove(resourceFromDb);

                await _db.SaveChangesAsync();
            }
        }
    }
}
