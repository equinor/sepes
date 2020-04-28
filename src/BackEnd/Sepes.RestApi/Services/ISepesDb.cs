using Sepes.Infrastructure.Model.SepesSqlModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sepes.Infrastructure.Dto;

namespace Sepes.RestApi.Services
{
    public interface ISepesDb
    {
        Task<string> getDatasetList();
        Task<StudyDto> NewStudy(StudyDto study);
        Task<bool> UpdateStudy(StudyDto study);
        Task<IEnumerable<StudyDto>> GetAllStudies();
    }
}