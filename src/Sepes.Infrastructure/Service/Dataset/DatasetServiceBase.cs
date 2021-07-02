using AutoMapper;
using Microsoft.Extensions.Logging;
using Sepes.Common.Dto.Dataset;
using Sepes.Common.Interface;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using System;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class DatasetServiceBase
    {
        protected readonly ILogger _logger;
        protected readonly SepesDbContext _db;
        protected readonly IMapper _mapper;

        protected readonly IUserService _userService;
        protected readonly IStudyPermissionService _studyPermissionService;

        public DatasetServiceBase(SepesDbContext db, IMapper mapper, ILogger logger, IUserService userService, IStudyPermissionService studyPermissionService)        
        {
            _logger = logger;
            _db = db;
            _mapper = mapper;
            _userService = userService;
            _studyPermissionService = studyPermissionService;
        }      

        protected async Task SoftDeleteAsync(Dataset dataset)
        {
            await SoftDeleteUtil.MarkAsDeleted(dataset, _userService);           
            await _db.SaveChangesAsync();
        }

        protected async Task HardDeleteAsync(Dataset dataset)
        {
            dataset.StudyDatasets.Clear();
            _db.Datasets.Remove(dataset);
            await _db.SaveChangesAsync();
        }     

        protected void PerformUsualTestForPostedDatasets(DatasetDto datasetDto)
        {
            if (String.IsNullOrWhiteSpace(datasetDto.Name))
            {
                throw new ArgumentException($"Field Dataset.Name is required. Current value: {datasetDto.Name}");
            }
            if (String.IsNullOrWhiteSpace(datasetDto.Classification))
            {
                throw new ArgumentException($"Field Dataset.Classification is required. Current value: {datasetDto.Classification}");
            }
            if (String.IsNullOrWhiteSpace(datasetDto.Location))
            {
                throw new ArgumentException($"Field Dataset.Location is required. Current value: {datasetDto.Location}");
            }
        }

        protected async Task DecorateDtoStudySpecific(IUserService userService, Study studyDb, DatasetPermissionsDto dto)
        {
            var currentUser = await userService.GetCurrentUserAsync();
            var studyPermissionDetails = _mapper.Map<IHasStudyPermissionDetails>(studyDb);

            dto.EditDataset = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_AddRemove_Dataset);
            dto.DeleteDataset = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_AddRemove_Dataset);
        }

        protected async Task DecorateDtoPreApproved(IUserService userService, Study studyDb, DatasetPermissionsDto dto)
        {
            var currentUser = await userService.GetCurrentUserAsync();
            var studyPermissionDetails = _mapper.Map<IHasStudyPermissionDetails>(studyDb);

            dto.EditDataset = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.PreApprovedDataset_Create_Update_Delete);
            dto.DeleteDataset = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.PreApprovedDataset_Create_Update_Delete);
        }
    }
}
