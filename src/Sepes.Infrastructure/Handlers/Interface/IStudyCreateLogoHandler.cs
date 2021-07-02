using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers.Interface
{
    public interface IStudyCreateLogoHandler
    { 
        Task<string> CreateAsync(int studyId, IFormFile studyLogo);
    }
}
