using Microsoft.AspNetCore.Http;
using Sepes.Infrastructure.Model;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.Interface
{
    public interface IStudyLogoCreateService
    { 
        Task<string> CreateAsync(Study study, IFormFile studyLogo);    
    }
}