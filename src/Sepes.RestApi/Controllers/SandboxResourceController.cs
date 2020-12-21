﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Service.Interface;
using System.Threading.Tasks;

namespace Sepes.RestApi.Controller
{
    [Route("api")]
    [ApiController]
    [Produces("application/json")]
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize]
    public class SandboxResourceController : ControllerBase
    {
        readonly ICloudResourceReadService _sandboxResourceService;
        readonly ISandboxResourceCreateService _sandboxResourceCreateService;
        readonly ISandboxResourceRetryService _sandboxResourceRetryService;

        public SandboxResourceController(ICloudResourceReadService sandboxResourceService, ISandboxResourceCreateService sandboxResourceCreateService, ISandboxResourceRetryService sandboxResourceRetryService)
        {
            _sandboxResourceService = sandboxResourceService;
            _sandboxResourceCreateService = sandboxResourceCreateService;
            _sandboxResourceRetryService = sandboxResourceRetryService;
        }       

        [HttpGet("sandboxes/{sandboxId}/resources")]
        public async Task<IActionResult> GetSandboxResources(int sandboxId)
        {
            var sandboxes = await _sandboxResourceService.GetSandboxResourcesLight(sandboxId);
            return new JsonResult(sandboxes);
        }

        [HttpPut("resources/{resourceId}/retry")]
        public async Task<IActionResult> RetryLastOperation(int resourceId)
        {
            var resource = await _sandboxResourceRetryService.RetryLastOperation(resourceId);
            return new JsonResult(resource);
        }

        [HttpPut("sandboxes/{sandboxId}/retryCreate")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> ReScheduleCreation(int sandboxId)
        {
            await _sandboxResourceRetryService.ReScheduleSandboxResourceCreation(sandboxId);
            return new NoContentResult();
        }
    }
}
