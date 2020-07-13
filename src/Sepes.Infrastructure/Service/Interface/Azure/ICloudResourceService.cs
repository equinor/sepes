﻿using Microsoft.Azure.Management.Network.Models;
using Sepes.Infrastructure.Dto;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public interface ICloudResourceService
    {
        
        Task<CloudResourceDto> Add(string resourceGroupId, string resourceGroupName, string type, string resourceId, string resourceName);

        Task<CloudResourceDto> Add(string resourceGroupId, string resourceGroupName, Resource resource);

        Task<CloudResourceDto> AddResourceGroup(string resourceGroupId, string resourceGroupName, string type);
        Task<CloudResourceDto> GetByIdAsync(int id);

        Task<CloudResourceDto> MarkAsDeletedByIdAsync(int id);


    }
}
