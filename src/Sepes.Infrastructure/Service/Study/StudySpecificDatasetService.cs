using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Common.Constants.CloudResource;
using Sepes.Common.Dto.Dataset;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class StudySpecificDatasetService : DatasetServiceBase, IStudySpecificDatasetService
    {
        readonly IStudyEfModelService _studyModelService;
        readonly IStudyWbsValidationService _studyWbsValidationService;
        readonly IStudySpecificDatasetModelService _studySpecificDatasetModelService;
        readonly IDatasetCloudResourceService _datasetCloudResourceService;

        public StudySpecificDatasetService(
            SepesDbContext db,
            IMapper mapper,
            ILogger<StudySpecificDatasetService> logger,
            IUserService userService,
            IStudyPermissionService studyPermissionService,
            IStudyEfModelService studyModelService,
            IStudyWbsValidationService studyWbsValidationService,
            IStudySpecificDatasetModelService studySpecificDatasetModelService,
            IDatasetCloudResourceService datasetCloudResourceService
            )
            : base(db, mapper, logger, userService, studyPermissionService)
        {
            _studyModelService = studyModelService ?? throw new ArgumentNullException(nameof(studyModelService));
            _studyWbsValidationService = studyWbsValidationService ?? throw new ArgumentNullException(nameof(studyWbsValidationService));
            _studySpecificDatasetModelService = studySpecificDatasetModelService;
            _datasetCloudResourceService = datasetCloudResourceService ?? throw new ArgumentNullException(nameof(datasetCloudResourceService));
        }

        public async Task<DatasetDto> CreateStudySpecificDatasetAsync(int studyId, DatasetCreateUpdateInputBaseDto newDatasetInput, string clientIp, CancellationToken cancellationToken = default)
        {
            var studyFromDb = await _studyModelService.GetForDatasetCreationAsync(studyId, UserOperation.Study_AddRemove_Dataset);

            await _studyWbsValidationService.ValidateForDatasetCreationOrThrow(studyFromDb);

            DatasetUtils.PerformUsualTestForPostedDatasets(newDatasetInput);

            ThrowIfDatasetNameTaken(studyFromDb, newDatasetInput.Name);

            var dataset = _mapper.Map<Dataset>(newDatasetInput);
            dataset.StudySpecific = true;

            var currentUser = await _userService.GetCurrentUserAsync();
            dataset.CreatedBy = currentUser.UserName;

            _db.Datasets.Add(dataset);
            await _db.SaveChangesAsync();

            // Create new linking table entry
            var studyDataset = new StudyDataset { StudyId = studyFromDb.Id, DatasetId = dataset.Id };
            await _db.StudyDatasets.AddAsync(studyDataset);
            await _db.SaveChangesAsync();

            dataset = await _studySpecificDatasetModelService.GetForResourceAndFirewall(dataset.Id, UserOperation.Study_AddRemove_Dataset);

            try
            {
                await _datasetCloudResourceService.CreateResourcesForStudySpecificDatasetAsync(studyFromDb, dataset, clientIp, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to create resources for Study specific Dataset, deleting dataset");
                _db.StudyDatasets.Remove(studyDataset);
                _db.Datasets.Remove(dataset);
                await _db.SaveChangesAsync();
                throw;
            }

            var datasetDto = _mapper.Map<DatasetDto>(dataset);

            await DecorateDtoStudySpecific(_userService, studyFromDb, datasetDto.Permissions);

            return datasetDto;
        }

        public async Task<DatasetDto> UpdateStudySpecificDatasetAsync(int studyId, int datasetId, DatasetCreateUpdateInputBaseDto updatedDataset)
        {
            DatasetUtils.PerformUsualTestForPostedDatasets(updatedDataset);

            var studyFromDb = await _studyModelService.GetForDatasetsAsync(studyId, UserOperation.Study_AddRemove_Dataset);

            var datasetFromDb = GetStudySpecificDatasetOrThrow(studyFromDb, datasetId);

            DatasetUtils.UpdateDatasetBasicDetails(datasetFromDb, updatedDataset);

            EntityValidationUtil.Validate<Dataset>(datasetFromDb);       

            await _db.SaveChangesAsync();

            var datasetDto = _mapper.Map<DatasetDto>(datasetFromDb);

            await DecorateDtoStudySpecific(_userService, studyFromDb, datasetDto.Permissions);

            return datasetDto;
        }

        Dataset GetStudySpecificDatasetOrThrow(Study study, int datasetId)
        {
            var studyDatasetRelation = study.StudyDatasets.FirstOrDefault(sd => sd.DatasetId == datasetId);

            if (studyDatasetRelation == null)
            {
                throw NotFoundException.CreateForEntity("StudyDataset", datasetId);
            }

            return studyDatasetRelation.Dataset;
        }

        void ThrowIfDatasetNameTaken(Study study, string datasetName)
        {
            foreach (var curStudyDataset in study.StudyDatasets)
            {
                if (curStudyDataset.Dataset.StudySpecific && curStudyDataset.StudyId == study.Id && curStudyDataset.Dataset.Name == datasetName)
                {
                    throw new Exception($"Dataset with name {datasetName} allready exists");
                }
            }
        }

        public async Task SoftDeleteAllStudySpecificDatasetsAsync(Study study, CancellationToken cancellationToken = default)
        {
            await DeleteAllStudySpecificDatasetsWithHandlerAsync(study, SoftDeleteStudySpecificDatasetAsync, cancellationToken);
        }

        public async Task HardDeleteAllStudySpecificDatasetsAsync(Study study, CancellationToken cancellationToken = default)
        {
            await DeleteAllStudySpecificDatasetsWithHandlerAsync(study, HardDeleteStudySpecificDatasetAsync, cancellationToken);
        }

        async Task DeleteAllStudySpecificDatasetsWithHandlerAsync(Study study, Func<Study, int, CancellationToken, Task> deleteHandler, CancellationToken cancellationToken = default)
        {
            var idsForStudySpecificDataset = await _db.StudyDatasets.Where(sd => sd.StudyId == study.Id && !sd.Dataset.Deleted && sd.Dataset.StudySpecific).Select(sd => sd.DatasetId).ToListAsync();
            var studySpecificDatasetsToDelete = new List<int>();

            if (idsForStudySpecificDataset.Any())
            {
                foreach (var studySpecificDataset in idsForStudySpecificDataset)
                {
                    studySpecificDatasetsToDelete.Add(studySpecificDataset);
                }
            }          

            if (studySpecificDatasetsToDelete.Any())
            {
                foreach (var curStudySpecificDatasetId in studySpecificDatasetsToDelete)
                {
                    await deleteHandler(study, curStudySpecificDatasetId, cancellationToken);
                }
            }
        }

        public async Task SoftDeleteStudySpecificDatasetAsync(int datasetId, CancellationToken cancellationToken = default)
        {
            var dataset = await _studySpecificDatasetModelService.GetForResourceAndFirewall(datasetId, UserOperation.Study_AddRemove_Dataset);
            var study = dataset.StudyDatasets.SingleOrDefault().Study;
            await _datasetCloudResourceService.DeleteResourcesForStudySpecificDatasetAsync(study, dataset, cancellationToken);
            await SoftDeleteAsync(dataset);
        }

        public async Task SoftDeleteStudySpecificDatasetAsync(Study study, int datasetId, CancellationToken cancellationToken = default)
        {
            var dataset = await _studySpecificDatasetModelService.GetForResourceAndFirewall(datasetId, UserOperation.Study_AddRemove_Dataset);
            await _datasetCloudResourceService.DeleteResourcesForStudySpecificDatasetAsync(study, dataset, cancellationToken);
            await SoftDeleteAsync(dataset);
        }

        public async Task HardDeleteStudySpecificDatasetAsync(Study study, int datasetId, CancellationToken cancellationToken = default)
        {
            var dataset = await _studySpecificDatasetModelService.GetForResourceAndFirewall(datasetId, UserOperation.Study_AddRemove_Dataset);
            await _datasetCloudResourceService.DeleteResourcesForStudySpecificDatasetAsync(study, dataset, cancellationToken);
            await HardDeleteAsync(dataset);
        }

        public async Task HardDeleteStudySpecificDatasetAsync(int datasetId, CancellationToken cancellationToken = default)
        {
            var dataset = await _studySpecificDatasetModelService.GetForResourceAndFirewall(datasetId, UserOperation.Study_AddRemove_Dataset);
            var study = dataset.StudyDatasets.SingleOrDefault().Study;
            await _datasetCloudResourceService.DeleteResourcesForStudySpecificDatasetAsync(study, dataset, cancellationToken);
            await HardDeleteAsync(dataset);
        }

        public async Task DeleteAllStudyRelatedResourcesAsync(Study study, CancellationToken cancellationToken = default)
        {
            await _datasetCloudResourceService.DeleteAllStudyRelatedResourcesAsync(study, cancellationToken);
        }

        public async Task<List<DatasetResourceLightDto>> GetDatasetResourcesAsync(int studyId, int datasetId, CancellationToken cancellation)
        {
            var dataset = await _studySpecificDatasetModelService.GetForResourceAndFirewall(datasetId, UserOperation.Study_Read);

            //Filter out deleted resources
            var resourcesFiltered = dataset.Resources
                .Where(r => !SoftDeleteUtil.IsMarkedAsDeleted(r)
                    || (
                    SoftDeleteUtil.IsMarkedAsDeleted(r)
                    && !r.Operations.Where(o => o.OperationType == CloudResourceOperationType.DELETE && o.Status == CloudResourceOperationState.DONE_SUCCESSFUL).Any())

                ).ToList();

            var resourcesMapped = _mapper.Map<List<DatasetResourceLightDto>>(resourcesFiltered);

            return resourcesMapped;
        }
    }
}