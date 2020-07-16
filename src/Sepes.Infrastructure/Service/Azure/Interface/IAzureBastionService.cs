﻿using Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public interface IAzureBastionService
    {
      
        Task<BastionHost> Create(Region region, string resourceGroupName, string studyName, string sandboxName, string subnetId);
        Task Delete(string resourceGroupName, string bastionHostName);
        Task<bool> Exists(string resourceGroupName, string bastionHostName);
    }
}