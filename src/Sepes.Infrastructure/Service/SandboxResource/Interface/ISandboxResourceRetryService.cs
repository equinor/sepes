﻿using Sepes.Common.Response.Sandbox;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.Interface
{
    public interface ISandboxResourceRetryService
    {
        Task<SandboxResourceLight> RetryResourceFailedOperation(int resourceId);   
    }   
}
