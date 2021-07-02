using Sepes.Azure.Service.Interface;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class StudyLogoDeleteService : IStudyLogoDeleteService
    {
        readonly string _containerName = "studylogos";
        readonly IAzureBlobStorageService _azureBlobStorageService;
        readonly IAzureBlobStorageUriBuilderService _azureStorageAccountTokenService;

        public StudyLogoDeleteService(
            IAzureBlobStorageService blobService,
            IAzureBlobStorageUriBuilderService azureStorageAccountTokenService)
        {        
            _azureBlobStorageService = blobService;
            _azureStorageAccountTokenService = azureStorageAccountTokenService;

            _azureBlobStorageService.SetConnectionParameters(ConfigConstants.STUDY_LOGO_STORAGE_CONSTRING);
            _azureStorageAccountTokenService.SetConnectionParameters(ConfigConstants.STUDY_LOGO_STORAGE_CONSTRING);
        }       

        public async Task DeleteAsync(string logoUrl)
        {
            if (!String.IsNullOrWhiteSpace(logoUrl))
            {
                _ = await _azureBlobStorageService.DeleteFileFromBlobContainer(_containerName, logoUrl);
            }
        } 
    }
}
