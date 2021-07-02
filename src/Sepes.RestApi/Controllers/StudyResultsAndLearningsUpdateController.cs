using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Sepes.Common.Dto.Study;
using Sepes.Infrastructure.Handlers.Interface;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Sepes.RestApi.Controller
{
    [Route("api/studies")]
    [ApiController]
    [Produces("application/json")]
    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize]
    public class StudyResultsAndLearningsUpdateController : ControllerBase
    { 
        readonly IStudyResultsAndLearningsUpdateHandler _studyResultsAndLearningsUpdateHandler;
       

        public StudyResultsAndLearningsUpdateController(IStudyResultsAndLearningsUpdateHandler studyResultsAndLearningsUpdateHandler)
        {
            _studyResultsAndLearningsUpdateHandler = studyResultsAndLearningsUpdateHandler;
        } 
      

        [HttpPut("{studyId}/resultsandlearnings")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateResultsAndLearningsAsync(int studyId, StudyResultsAndLearningsDto resultsAndLearnings)
        {
            var resultsAndLearningsFromDb = await _studyResultsAndLearningsUpdateHandler.UpdateAsync(studyId, resultsAndLearnings);
            return new JsonResult(resultsAndLearningsFromDb);
        }
    }
}
