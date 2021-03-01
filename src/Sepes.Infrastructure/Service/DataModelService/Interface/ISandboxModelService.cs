﻿using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Model;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService.Interface
{
    public interface ISandboxModelService
    {
        Task<Sandbox> AddAsync(Sandbox sandbox);  

        Task<Sandbox> GetByIdAsync(int id, UserOperation userOperation, bool withIncludes = false, bool disableTracking = false);

        Task<string> GetRegionByIdAsync(int id, UserOperation userOperation);

        Task<Sandbox> GetByIdWithoutPermissionCheckAsync(int id);
    }
}
