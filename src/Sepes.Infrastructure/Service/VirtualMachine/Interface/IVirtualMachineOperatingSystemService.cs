﻿using Sepes.Common.Dto.VirtualMachine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.Interface
{
    public interface IVirtualMachineOperatingSystemService
    {  
        Task<List<VmOsDto>> AvailableOperatingSystems(int sandboxId, CancellationToken cancellationToken = default);
        Task<List<VmOsDto>> AvailableOperatingSystems(string region, CancellationToken cancellationToken = default);
    }
}
