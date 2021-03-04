﻿using Sepes.Infrastructure.Dto.Provisioning;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public interface IPerformResourceProvisioning
    {
        Task<ResourceProvisioningResult> EnsureCreated(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default);

        Task<ResourceProvisioningResult> GetSharedVariables(ResourceProvisioningParameters parameters);

        Task<ResourceProvisioningResult> Update(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default);

        Task<ResourceProvisioningResult> EnsureDeleted(ResourceProvisioningParameters parameters);
    }
}
