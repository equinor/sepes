﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Constants.CloudResource;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Dto.Sandbox;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Query;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class CloudResourceReadService : CloudResourceServiceBase, ICloudResourceReadService
    { 
        public CloudResourceReadService(SepesDbContext db, IConfiguration config, IMapper mapper, ILogger<CloudResourceReadService> logger, IUserService userService)
         : base(db, config, mapper, logger, userService)
        { 
        } 
        
        public async Task<CloudResource> GetByIdAsync(int id)
        {
            var entityFromDb = await GetOrThrowAsync(id);
            return entityFromDb;
        }  

        public async Task<CloudResource> GetOrThrowAsync(int id)
        {
            return await GetOrThrowInternalAsync(id);
        }

        public async Task<List<SandboxResourceLightDto>> GetSandboxResourcesLight(int sandboxId)
        {
            var sandboxFromDb = await GetOrThrowAsync(sandboxId, UserOperation.Study_Read, true);

            //Filter out deleted resources
            var resourcesFiltered = sandboxFromDb.Resources
                .Where(r => SoftDeleteUtil.IsMarkedAsDeleted(r) == false
                    || (
                    SoftDeleteUtil.IsMarkedAsDeleted(r)
                    && r.Operations.Where(o => o.OperationType == CloudResourceOperationType.DELETE && o.Status == CloudResourceOperationState.DONE_SUCCESSFUL).Any() == false)

                ).ToList();

            var resourcesMapped = _mapper.Map<List<SandboxResourceLightDto>>(resourcesFiltered);

            return resourcesMapped;
        }      

        public async Task<List<CloudResource>> GetAllActiveResources() => await _db.CloudResources.Include(sr => sr.Sandbox)
                                                                                                   .ThenInclude(sb => sb.Study)
                                                                                                    .Include(sr => sr.Operations)
                                                                                                   .Where(sr => !sr.Deleted)
                                                                                                   .ToListAsync();

       
       

        public async Task<IEnumerable<CloudResource>> GetDeletedResourcesAsync() => await _db.CloudResources.Include(sr => sr.Operations).Where(sr => sr.DeletedAt.HasValue && sr.DeletedAt.Value.AddMinutes(10) < DateTime.UtcNow)
                                                                                                                .ToListAsync();

        public async Task<bool> ResourceIsDeleted(int resourceId)
        {
            var resource = await _db.CloudResources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == resourceId);

            if(resource == null)
            {
                return true;
            }

            return SoftDeleteUtil.IsMarkedAsDeleted(resource);
        }

        public async Task<List<CloudResourceDto>> GetSandboxResources(int sandboxId, CancellationToken cancellation = default)
        {
            var queryable = CloudResourceQueries.GetSandboxResourcesQueryable(_db, sandboxId);

            var resources = await queryable.ToListAsync(cancellation);

            return _mapper.Map<List<CloudResourceDto>>(resources);
        }
    }
}
