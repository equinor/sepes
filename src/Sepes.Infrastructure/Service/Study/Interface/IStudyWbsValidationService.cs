using System.Threading.Tasks;
using Sepes.Infrastructure.Model;

namespace Sepes.Infrastructure.Service.Interface
{
    public interface IStudyWbsValidationService
    {
        Task ValidateForStudyCreate(Study study);

        Task ValidateForStudyUpdate(Study study);

        Task ValidateForSandboxCreationOrThrow(Study study);

        Task ValidateForDatasetCreationOrThrow(Study study);    
    }
}