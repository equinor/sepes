using Sepes.Common.Dto.Study;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers.Interface
{
    public interface IStudyResultsAndLearningsUpdateHandler
    { 
        Task<StudyResultsAndLearningsDto> UpdateAsync(int studyId, StudyResultsAndLearningsDto resultsAndLearnings);
    }
}
