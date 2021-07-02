using Microsoft.AspNetCore.Http;
using Sepes.Common.Dto.Study;
using Sepes.Infrastructure.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers.Interface
{
    public interface IStudyUpdateHandler
    { 
        Task<Study> UpdateAsync(int studyId, StudyUpdateDto newStudy, IFormFile logo = null, CancellationToken cancellationToken = default);
    }
}
