﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Dto;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Sepes.RestApi.Controller
{
    public partial class StudyController : StudyControllerBase
    {
        [HttpGet("{studyId}/sandboxes")]
        [Authorize(Roles = AppRoles.Admin)]
        //TODO: Must also be possible for sponsor rep and other roles
        public async Task<IActionResult> GetSandboxesByStudyIdAsync(int studyId)
        {
            var sandboxes = await _sandboxService.GetSandboxesForStudyAsync(studyId);
            return new JsonResult(sandboxes);
        }

        [HttpGet("{studyId}/sandboxes/{sandboxId}")]
        [Authorize(Roles = AppRoles.Admin)]
        //TODO: Must also be possible for sponsor rep and other roles
        public async Task<IActionResult> GetSandbox(int studyId, int sandboxId)
        {
            var sandboxes = await _sandboxService.GetSandbox(studyId, sandboxId);
            return new JsonResult(sandboxes);
        }

        [HttpPost("{studyId}/sandboxes")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Authorize(Roles = RoleSets.AdminOrSponsor)]
        //TODO: Must also be possible for sponsor rep/vendor admin
        public async Task<IActionResult> CreateSandboxAsync(int studyId, SandboxCreateDto newSandbox)
        {
            var updatedStudy = await _sandboxService.CreateAsync(studyId, newSandbox);
            return new JsonResult(updatedStudy);
        }

        [HttpDelete("{studyId}/sandboxes/{sandboxId}")]
        [Authorize(Roles = AppRoles.Admin)]
        //TODO: Must also be possible for sponsor rep/vendor admin
        public async Task<IActionResult> RemoveSandboxAsync(int studyId, int sandboxId)
        {
            var updatedStudy = await _sandboxService.DeleteAsync(studyId, sandboxId);
            return new JsonResult(updatedStudy);
        }

        [HttpGet("{studyId}/sandboxes/{sandboxId}/resources")]
        [Authorize(Roles = AppRoles.Admin)]
        //TODO: Must also be possible for sponsor rep and other roles
        public async Task<IActionResult> GetSandboxResources(int studyId, int sandboxId)
        {
            var sandboxes = await _sandboxService.GetSandboxResources(studyId, sandboxId);
            return new JsonResult(sandboxes);
        }

        [HttpGet("sandboxes/templatelookup")]
        [Authorize(Roles = AppRoles.Admin)]
        //TODO: Must also be possible for sponsor rep and other roles
        public async Task<IActionResult> GetTemplatesLookupAsync()
        {
            var templates = new List<LookupDto>();
            return new JsonResult(templates);
        }

        [HttpPut("{studyId}/sandboxes/{sandboxId}/rescheduleCreation")]
        [Authorize(Roles = AppRoles.Admin)]    
        public async Task<IActionResult> GetTemplatesLookupAsync(int studyId, int sandboxId)
        {
            await _sandboxService.ReScheduleSandboxCreation(studyId, sandboxId);
            return new NoContentResult();
        }


    }

}
