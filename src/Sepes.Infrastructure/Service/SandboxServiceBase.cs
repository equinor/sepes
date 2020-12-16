﻿using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Dto.Sandbox;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Service.Queries;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class SandboxServiceBase
    {
        protected readonly IConfiguration _configuration;
        protected readonly SepesDbContext _db;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;
        protected readonly IUserService _userService;    


        public SandboxServiceBase(IConfiguration configuration, SepesDbContext db, IMapper mapper, ILogger logger, IUserService userService)
        {
            _configuration = configuration;
            _db = db;
            _logger = logger;
            _mapper = mapper;          
            _userService = userService;

        } 
        
        protected async Task<Sandbox> GetOrThrowAsync(int sandboxId, UserOperation userOperation, bool withIncludes)
        {
            var sandbox = await SandboxSingularQueries.GetSandboxByIdCheckAccessOrThrow(_db, _userService, sandboxId, userOperation, withIncludes);

            if (sandbox == null)
            {
                throw NotFoundException.CreateForEntity("Sandbox", sandboxId);
            }

            return sandbox;
        }

        protected async Task<SandboxDto> GetDtoAsync(int sandboxId, UserOperation userOperation)
        {
            var sandboxFromDb = await GetOrThrowAsync(sandboxId, userOperation, false);
            var sandboxDto = _mapper.Map<SandboxDto>(sandboxFromDb);
            return sandboxDto;
        }
    }
}
