using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sepes.Infrastructure.Handlers.Interface;
using System.Threading.Tasks;

namespace Sepes.RestApi.Controller
{
    [Route("api/studies")]
    [ApiController]
    [Produces("application/json")]
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize]
    public class StudyLogoCreateController : ControllerBase
    {      
        readonly IStudyCreateLogoHandler _studyCreateLogoHandler;

        public StudyLogoCreateController(IStudyCreateLogoHandler studyCreateLogoHandler)
        {
            _studyCreateLogoHandler = studyCreateLogoHandler;
        }       

        // For local development, this method requires a running instance of Azure Storage Emulator
        [HttpPut("{studyId}/logo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(int studyId, [FromForm(Name = "image")] IFormFile studyLogo)
        {
            var logoUrl = await _studyCreateLogoHandler.CreateAsync(studyId, studyLogo);
            return new JsonResult(logoUrl);
        }
    }
}
