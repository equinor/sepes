﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Sepes.Infrastructure.Service.Interface;
using System.Threading.Tasks;

namespace Sepes.RestApi.Controller
{
    [Route("api/studies")]
    [ApiController]
    [Produces("application/json")]
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize]
    public class StudyListController : ControllerBase
    {
        readonly IStudyRawQueryReadService _studyRawQueryReadService;
     

        public StudyListController(IStudyRawQueryReadService studyRawQueryReadService)
        {
            _studyRawQueryReadService = studyRawQueryReadService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStudiesAsync()
        {
            var studies = await _studyRawQueryReadService.GetStudyListAsync();
            return new JsonResult(studies);
        }

           
    }
}
