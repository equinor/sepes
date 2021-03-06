﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class CloudResourceServiceBase : EfModelServiceBase<CloudResource>
    {
        protected readonly IMapper _mapper;

        public CloudResourceServiceBase(SepesDbContext db, IConfiguration config, IMapper mapper, ILogger logger, IStudyPermissionService studyPermissionService)
         : base(config, db, logger, studyPermissionService)
        {
            _mapper = mapper;
        }

        protected IQueryable<CloudResource> BaseQueryable()
        {
            return _db.CloudResources
                                   .Include(r => r.Sandbox)
                                       .ThenInclude(s => s.Resources)
                                   .Include(r => r.Operations);
        }

        protected IQueryable<CloudResource> AddAccessCheckIncludes(IQueryable<CloudResource> source)
        {
            return source.Include(r => r.Sandbox)
                                       .ThenInclude(s => s.Study)
                                       .ThenInclude(s => s.StudyParticipants);
        }

        protected IQueryable<CloudResource> AddNotDeletedFilter(IQueryable<CloudResource> source)
        {
            return source.Where(r=> r.Deleted == false);
        }
      

        protected async Task<CloudResource> GetInternalWithoutAccessCheckAsync(int id, bool readOnly = false, bool onlyNonDeleted = false, bool throwIfNotFound = false, bool includeAccessCheckEntities = false)
        {
            var queryable = BaseQueryable();

            if (includeAccessCheckEntities)
            {
                queryable = AddAccessCheckIncludes(queryable);
            }

            queryable = queryable.Where(r=> r.Id == id);

            if (onlyNonDeleted)
            {
                queryable = AddNotDeletedFilter(queryable);
            }

            if (readOnly)
            {
                queryable = queryable.AsNoTracking();
            }

            var entityFromDb = await queryable.SingleOrDefaultAsync();

            if (entityFromDb == null && throwIfNotFound)
            {
                throw NotFoundException.CreateForEntity("CloudResource", id);
            }            

            return entityFromDb;
        }

        protected async Task<CloudResource> GetInternalAsync(int id, UserOperation userOperation, bool readOnly = false, bool onlyNonDeleted = false, bool throwIfNotFound = false)
        {
            var resource = await GetInternalWithoutAccessCheckAsync(id, readOnly, onlyNonDeleted, throwIfNotFound, true);

            await CheckAccesAndThrowIfNotAllowed(resource.Sandbox.Study, userOperation);

            return resource;
        }        

        protected CloudResourceDto MapEntityToDto(CloudResource entity) => _mapper.Map<CloudResourceDto>(entity);

    }
}
