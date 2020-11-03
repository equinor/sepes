﻿using Sepes.Infrastructure.Dto;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.Interface
{
    public interface IUserPermissionService
    {
        Task<UserPermissionDto> GetUserPermissionsAsync();
    }
}