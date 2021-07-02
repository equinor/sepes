using AutoMapper;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Dataset;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class DatasetService : DatasetServiceBase, IDatasetService
    {
        readonly IPreApprovedDatasetModelService _preApprovedDatasetModelService;
        public DatasetService(SepesDbContext db, IMapper mapper, ILogger<DatasetService> logger, IUserService userService, IStudyPermissionService studyPermissionService, IPreApprovedDatasetModelService preApprovedDatasetModelService)
            : base(db, mapper, logger, userService, studyPermissionService)
        {
            _preApprovedDatasetModelService = preApprovedDatasetModelService;
        }

        public async Task<DatasetDto> GetByIdAsync(int datasetId)
        {
            var datasetFromDb = await _preApprovedDatasetModelService.GetByIdAsync(datasetId, UserOperation.PreApprovedDataset_Read);         

            var datasetDto = _mapper.Map<DatasetDto>(datasetFromDb);

            return datasetDto;
        }

        public async Task<IEnumerable<DatasetLookupItemDto>> GetLookupAsync()
        {
            var datasetsFromDb = await _preApprovedDatasetModelService.GetAllAsync(UserOperation.PreApprovedDataset_Read);
            var datasetDtos = _mapper.Map<IEnumerable<DatasetLookupItemDto>>(datasetsFromDb);

            return datasetDtos;
        }

        public async Task<IEnumerable<DatasetDto>> GetAllAsync()
        {
            var datasetsFromDb = await _preApprovedDatasetModelService.GetAllAsync(UserOperation.PreApprovedDataset_Read);
            var datasetDtos = _mapper.Map<IEnumerable<DatasetDto>>(datasetsFromDb);
            return datasetDtos;
        }        

        public async Task<DatasetDto> CreateAsync(PreApprovedDatasetCreateUpdateDto newDataset)
        {
            var newDatasetDbModel = _mapper.Map<Dataset>(newDataset);
            var dataset = await _preApprovedDatasetModelService.CreateAsync(newDatasetDbModel);        

            return _mapper.Map<DatasetDto>(dataset);
        }

        public async Task<DatasetDto> UpdateAsync(int datasetId, DatasetDto updatedDataset)
        {
            var datasetFromDb = await _preApprovedDatasetModelService.GetByIdAsync(datasetId, UserOperation.PreApprovedDataset_Create_Update_Delete);

            PerformUsualTestForPostedDatasets(updatedDataset);

            if (!String.IsNullOrWhiteSpace(updatedDataset.Name) && updatedDataset.Name != datasetFromDb.Name)
            {
                datasetFromDb.Name = updatedDataset.Name;
            }
            if (!String.IsNullOrWhiteSpace(updatedDataset.Location) && updatedDataset.Location != datasetFromDb.Location)
            {
                datasetFromDb.Location = updatedDataset.Location;
            }
            if (!String.IsNullOrWhiteSpace(updatedDataset.Classification) && updatedDataset.Classification != datasetFromDb.Classification)
            {
                datasetFromDb.Classification = updatedDataset.Classification;
            }
            if (!String.IsNullOrWhiteSpace(updatedDataset.StorageAccountName) && updatedDataset.StorageAccountName != datasetFromDb.StorageAccountName)
            {
                datasetFromDb.StorageAccountName = updatedDataset.StorageAccountName;
            }
            if (updatedDataset.LRAId != datasetFromDb.LRAId)
            {
                datasetFromDb.LRAId = updatedDataset.LRAId;
            }
            if (updatedDataset.DataId != datasetFromDb.DataId)
            {
                datasetFromDb.DataId = updatedDataset.DataId;
            }
            if (updatedDataset.SourceSystem != datasetFromDb.SourceSystem)
            {
                datasetFromDb.SourceSystem = updatedDataset.SourceSystem;
            }
            if (updatedDataset.BADataOwner != datasetFromDb.BADataOwner)
            {
                datasetFromDb.BADataOwner = updatedDataset.BADataOwner;
            }
            if (updatedDataset.Asset != datasetFromDb.Asset)
            {
                datasetFromDb.Asset = updatedDataset.Asset;
            }
            if (updatedDataset.CountryOfOrigin != datasetFromDb.CountryOfOrigin)
            {
                datasetFromDb.CountryOfOrigin = updatedDataset.CountryOfOrigin;
            }
            if (updatedDataset.AreaL1 != datasetFromDb.AreaL1)
            {
                datasetFromDb.AreaL1 = updatedDataset.AreaL1;
            }
            if (updatedDataset.AreaL2 != datasetFromDb.AreaL2)
            {
                datasetFromDb.AreaL2 = updatedDataset.AreaL2;
            }
            if (updatedDataset.Tags != datasetFromDb.Tags)
            {
                datasetFromDb.Tags = updatedDataset.Tags;
            }
            if (updatedDataset.Description != datasetFromDb.Description)
            {
                datasetFromDb.Description = updatedDataset.Description;
            }
            datasetFromDb.Updated = DateTime.UtcNow;
            EntityValidationUtil.Validate<Dataset>(datasetFromDb);         
            await _db.SaveChangesAsync();
            return await GetByIdAsync(datasetFromDb.Id);
        }       

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task DeleteAsync(int datasetId)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException("Delete of Pre-Approved datasets not implemented yet");
        }    
    }
}
