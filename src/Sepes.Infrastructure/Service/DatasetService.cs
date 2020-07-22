﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class DatasetService : ServiceBase<Dataset>, IDatasetService
    {
        readonly IStudyService _studyService;

        public DatasetService(SepesDbContext db, IMapper mapper, IStudyService studyService)
            :base(db, mapper)
        {            
            _studyService = studyService;
        }

        public async Task<IEnumerable<DatasetListItemDto>> GetDatasetsLookupAsync()
        {
            var datasetsFromDb = await _db.Datasets
                .Where(ds => ds.StudyNo == null)
                .ToListAsync();
            var dataasetsDtos = _mapper.Map<IEnumerable<DatasetListItemDto>>(datasetsFromDb);

            return dataasetsDtos;  
        }

        public async Task<IEnumerable<DatasetDto>> GetDatasetsAsync()
        {
            var datasetsFromDb = await _db.Datasets
                .Where(ds => ds.StudyNo == null)
                .ToListAsync();
            var dataasetDtos = _mapper.Map<IEnumerable<DatasetDto>>(datasetsFromDb);

            return dataasetDtos;
        }

        public async Task<DatasetDto> GetDatasetByDatasetIdAsync(int datasetId)
        {
            var datasetFromDb = await GetDatasetOrThrowAsync(datasetId);

            var datasetDto = _mapper.Map<DatasetDto>(datasetFromDb);

            return datasetDto;
        }

        public async Task<DatasetDto> GetDatasetByStudyIdAndDatasetIdAsync(int studyId, int datasetId)
        {
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);

            var studyDatasetRelation = studyFromDb.StudyDatasets.FirstOrDefault(sd => sd.DatasetId == datasetId);

            if (studyDatasetRelation == null)
            {
                throw NotFoundException.CreateForIdentity("Dataset", datasetId);
            }

            var datasetDto = _mapper.Map<DatasetDto>(studyDatasetRelation.Dataset);

            return datasetDto;
        }

        async Task<Dataset> GetDatasetOrThrowAsync(int id)
        {
            var datasetFromDb = await _db.Datasets
                .Where(ds => ds.StudyNo == null)
                .Include(s => s. StudyDatasets)
                .ThenInclude(sd=> sd.Study)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (datasetFromDb == null)
            {
                throw NotFoundException.CreateForIdentity("Dataset", id);
            }

            return datasetFromDb;
        }

        public async Task<StudyDto> AddDatasetToStudyAsync(int studyId, int datasetId)
        {
            // Run validations: (Check if both id's are valid)
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);
            var datasetFromDb = await _db.Datasets.FirstOrDefaultAsync(ds => ds.Id == datasetId);

            if (datasetFromDb == null)
            {
                throw NotFoundException.CreateForIdentity("Dataset", datasetId);
            }

            if (datasetFromDb.StudyNo != null)
            {
                throw new ArgumentException($"Dataset with id {datasetId} is studySpecific, and cannot be linked using this method.");
            }

            // Create new linking table
            StudyDataset studyDataset = new StudyDataset { Study = studyFromDb, Dataset = datasetFromDb };
            await _db.StudyDatasets.AddAsync(studyDataset);
            await _db.SaveChangesAsync();

            return await _studyService.GetStudyByIdAsync(studyId);
        }

        public async Task<StudyDto> RemoveDatasetFromStudyAsync(int studyId, int datasetId)
        {
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);
            var datasetFromDb = await _db.Datasets.FirstOrDefaultAsync(ds => ds.Id == datasetId);

            //Does dataset exist?
            if (datasetFromDb == null)
            {
                throw NotFoundException.CreateForIdentity("Dataset", datasetId);
            }

            var studyDatasetFromDb = await _db.StudyDatasets
                .FirstOrDefaultAsync(ds => ds.StudyId == studyId && ds.DatasetId == datasetId);

            //Is dataset linked to a study?
            if (studyDatasetFromDb == null)
            {
                throw NotFoundException.CreateForIdentity("StudyDataset", datasetId);
            }

            _db.StudyDatasets.Remove(studyDatasetFromDb);

            //If dataset is studyspecific, remove dataset as well.
            // Possibly keep database entry, but mark as deleted.
            if (datasetFromDb.StudyNo != null)
            {
                _db.Datasets.Remove(datasetFromDb);
            }

            await _db.SaveChangesAsync();
            var retVal = await _studyService.GetStudyByIdAsync(studyId);
            return retVal;
        }

        public async Task<StudyDto> AddStudySpecificDatasetAsync(int studyId, StudySpecificDatasetDto newDataset)
        {
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);
            PerformUsualTestForPostedDatasets(newDataset);
            var dataset = _mapper.Map<Dataset>(newDataset);
            dataset.StudyNo = studyId;
            await _db.Datasets.AddAsync(dataset);

            // Create new linking table
            StudyDataset studyDataset = new StudyDataset { Study = studyFromDb, Dataset = dataset };
            await _db.StudyDatasets.AddAsync(studyDataset);
            await _db.SaveChangesAsync();

            return await _studyService.GetStudyByIdAsync(studyId);
        }

        void PerformUsualTestForPostedDatasets(StudySpecificDatasetDto datasetDto)
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
            if (String.IsNullOrWhiteSpace(datasetDto.StorageAccountName))
            {
                throw new ArgumentException($"Field Dataset.StorageAccountName is required. Current value: {datasetDto.StorageAccountName}");
            }
        }

        void PerformUsualTestForPostedDatasets(DatasetDto datasetDto)
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

        public async Task<DatasetDto> CreateDatasetAsync(DatasetDto newDataset)
        {
            var newDatasetDbModel = _mapper.Map<Dataset>(newDataset);
            var newDatasetId = await Add(newDatasetDbModel);
            return await GetDatasetByDatasetIdAsync(newDatasetId);

        }

        public async Task<DatasetDto> UpdateDatasetAsync(int datasetId, DatasetDto updatedDataset)
        {
            PerformUsualTestForPostedDatasets(updatedDataset);
            var datasetFromDb = await GetDatasetOrThrowAsync(datasetId);
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
            Validate(datasetFromDb);
            await _db.SaveChangesAsync();
            return await GetDatasetByDatasetIdAsync(datasetFromDb.Id);
        }

        public async Task<DatasetDto> UpdateStudySpecificDatasetAsync(int studyId, int datasetId, StudySpecificDatasetDto updatedDataset)
        {
            PerformUsualTestForPostedDatasets(updatedDataset);
            var datasetFromDb = await GetStudySpecificDatasetOrThrowAsync(studyId, datasetId);
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
            //if (!String.IsNullOrWhiteSpace(updatedDataset.StorageAccountName) && updatedDataset.StorageAccountName != datasetFromDb.StorageAccountName)
            //{
            //    datasetFromDb.StorageAccountName = updatedDataset.StorageAccountName;
            //}
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
            Validate(datasetFromDb);
            await _db.SaveChangesAsync();
            return await GetDatasetByStudyIdAndDatasetIdAsync(studyId, datasetFromDb.Id);
        }

        async Task<Dataset> GetStudySpecificDatasetOrThrowAsync(int studyId, int datasetId)
        {
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);

            var studyDatasetRelation = studyFromDb.StudyDatasets.FirstOrDefault(sd => sd.DatasetId == datasetId);

            if (studyDatasetRelation == null)
            {
                throw NotFoundException.CreateForIdentity("Dataset", datasetId);
            }

            return studyDatasetRelation.Dataset;
        }
    }
}
