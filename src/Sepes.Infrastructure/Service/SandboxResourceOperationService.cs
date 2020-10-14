﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sepes.Infrastructure.Constants.CloudResource;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class SandboxResourceOperationService : ISandboxResourceOperationService
    {
        readonly SepesDbContext _db;
        readonly IMapper _mapper;
        readonly IUserService _userService;

        public SandboxResourceOperationService(SepesDbContext db, IMapper mapper, IUserService userService)
        {
            _db = db;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<SandboxResourceOperationDto> AddAsync(int sandboxResourceId, SandboxResourceOperationDto operationDto)
        {
            var sandboxResourceFromDb = await GetSandboxResourceOrThrowAsync(sandboxResourceId);
            var newOperation = _mapper.Map<SandboxResourceOperation>(operationDto);
            
            sandboxResourceFromDb.Operations.Add(newOperation);
            await _db.SaveChangesAsync();
            return await GetByIdAsync(newOperation.Id);
        }

        public async Task<SandboxResourceOperationDto> GetByIdAsync(int id)
        {
            var itemFromDb = await GetOrThrowAsync(id);
            var itemDto = _mapper.Map<SandboxResourceOperationDto>(itemFromDb);
            return itemDto;
        }

        async Task<SandboxResourceOperation> GetOrThrowAsync(int id)
        {
            var entityFromDb = await _db.SandboxResourceOperations
                .Include(o => o.Resource)
                 .ThenInclude(r => r.Sandbox)
                         .ThenInclude(sb => sb.Study)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (entityFromDb == null)
            {
                throw NotFoundException.CreateForEntity("SandboxResourceOperation", id);
            }

            return entityFromDb;
        }

        public async Task<SandboxResourceOperationDto> UpdateStatusAsync(int id, string status, string updatedProvisioningState = null)
        {
            var currentUser = _userService.GetCurrentUser();

            var itemFromDb = await GetOrThrowAsync(id);
            itemFromDb.Status = status;
            itemFromDb.Updated = DateTime.UtcNow;
            itemFromDb.UpdatedBy = currentUser.UserName;           

            if (updatedProvisioningState != null)
            {  
                itemFromDb.Resource.LastKnownProvisioningState = updatedProvisioningState;
                itemFromDb.Resource.Updated = DateTime.UtcNow;
                itemFromDb.Resource.UpdatedBy = currentUser.UserName;
            }

            await _db.SaveChangesAsync();

            return await GetByIdAsync(itemFromDb.Id);
        }

        public async Task<SandboxResourceOperationDto> UpdateStatusAndIncreaseTryCountAsync(int id, string status)
        {
            var currentUser = _userService.GetCurrentUser();

            var itemFromDb = await GetOrThrowAsync(id);
            itemFromDb.Status = status;
            itemFromDb.TryCount++;
            itemFromDb.Updated = DateTime.UtcNow;
            itemFromDb.UpdatedBy = currentUser.UserName;
            await _db.SaveChangesAsync();

            return await GetByIdAsync(itemFromDb.Id);
        }

        public async Task<SandboxResourceOperationDto> SetInProgressAsync(int id, string requestId, string status)
        {
            var currentUser = _userService.GetCurrentUser();

            var itemFromDb = await GetOrThrowAsync(id);
            itemFromDb.TryCount++;
            itemFromDb.CarriedOutBySessionId = requestId;
            itemFromDb.Status = status;
            itemFromDb.Updated = DateTime.UtcNow;
            itemFromDb.UpdatedBy = currentUser.UserName;
            await _db.SaveChangesAsync();

            return await GetByIdAsync(itemFromDb.Id);
        }

        private async Task<SandboxResource> GetSandboxResourceOrThrowAsync(int id)
        {
            var entityFromDb = await _db.SandboxResources
                .Include(sr => sr.Operations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entityFromDb == null)
            {
                throw NotFoundException.CreateForEntity("AzureResource", id);
            }

            return entityFromDb;
        }

        public async Task<bool> ExistsPreceedingUnfinishedOperationsAsync(SandboxResourceOperationDto operationDto)
        {
           return await _db.SandboxResourceOperations.Where(o => o.SandboxResourceId == operationDto.Resource.Id.Value && o.BatchId != operationDto.BatchId && o.Created < operationDto.Created && (o.Status == CloudResourceOperationState.IN_PROGRESS || o.Status == CloudResourceOperationState.NOT_STARTED)).AnyAsync();
        }

        public async Task<bool> OperationIsFinishedAndSucceededAsync(int operationId)
        {
            var itemFromDb = await GetOrThrowAsync(operationId);

            return itemFromDb.Status == CloudResourceOperationState.DONE_SUCCESSFUL;
        }
    }
}
