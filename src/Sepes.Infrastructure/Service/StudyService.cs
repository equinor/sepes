﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Interface;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Sepes.Infrastructure.Service
{
    public class StudyService : ServiceBase<Study>, IStudyService
    {
        readonly IHasPrincipal _principalService;
        readonly IAzureBlobStorageService _azureBlobStorageService;      

        public StudyService(IHasPrincipal principalService, SepesDbContext db, IMapper mapper, IAzureBlobStorageService azureBlobStorageService)
            :base(db, mapper)
        {
            this._principalService = principalService;
            _azureBlobStorageService = azureBlobStorageService;
        }

        public async Task<IEnumerable<StudyListItemDto>> GetStudiesAsync(bool? includeRestricted = null)
        {
            List<Study> studiesFromDb;

            if (includeRestricted.HasValue && includeRestricted.Value)
            {
               var principal =  _principalService.GetPrincipal();

                if(principal == null)
                {
                    throw new ForbiddenException("Unknown user");
                }
             
                var studiesQueryable = GetStudiesIncludingRestrictedForCurrentUser(_db, principal.Identity.Name);
                studiesFromDb = await studiesQueryable.ToListAsync();
            }
            else
            {
              
                studiesFromDb = await _db.Studies.Where(s => !s.Restricted).ToListAsync();
            }

            var studiesDtos = _mapper.Map<IEnumerable<StudyListItemDto>>(studiesFromDb);

            studiesDtos = await _azureBlobStorageService.DecorateLogoUrlsWithSAS(studiesDtos);
            return studiesDtos;
        }  
        
        IQueryable<Study> GetStudiesIncludingRestrictedForCurrentUser(SepesDbContext db, string username)
        {
            return db.Studies.Where(s => s.Restricted == false || s.StudyParticipants.Where(sp => sp.Participant != null && sp.Participant.UserName == username).FirstOrDefault() != null);
        }

        public async Task<StudyDto> GetStudyByIdAsync(int studyId)
        {
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);
            var studyDto = _mapper.Map<StudyDto>(studyFromDb);

            studyDto = await _azureBlobStorageService.DecorateLogoUrlWithSAS(studyDto);

            return studyDto;
        }

        public async Task<StudyDto> CreateStudyAsync(StudyDto newStudy)
        {
            var newStudyDbModel = _mapper.Map<Study>(newStudy);

            var newStudyId = await Add(newStudyDbModel);       

            return await GetStudyByIdAsync(newStudyId);
        }

        public async Task<StudyDto> UpdateStudyDetailsAsync(int studyId, StudyDto updatedStudy)
        {
            PerformUsualTestsForPostedStudy(studyId, updatedStudy);

            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);

            if (!String.IsNullOrWhiteSpace(updatedStudy.Name) && updatedStudy.Name != studyFromDb.Name)
            {
                studyFromDb.Name = updatedStudy.Name;
            }

            if (updatedStudy.Description != studyFromDb.Description)
            {
                studyFromDb.Description = updatedStudy.Description;
            }

            if (!String.IsNullOrWhiteSpace(updatedStudy.Vendor) && updatedStudy.Vendor != studyFromDb.Vendor)
            {
                studyFromDb.Vendor = updatedStudy.Vendor;
            }

            if (updatedStudy.Restricted != studyFromDb.Restricted)
            {
                studyFromDb.Restricted = updatedStudy.Restricted;
            }

            if (updatedStudy.WbsCode != studyFromDb.WbsCode)
            {
                studyFromDb.WbsCode = updatedStudy.WbsCode;
            }

            if (updatedStudy.ResultsAndLearnings != studyFromDb.ResultsAndLearnings)
            {
                studyFromDb.ResultsAndLearnings = updatedStudy.ResultsAndLearnings;
            }

            studyFromDb.Updated = DateTime.UtcNow;

            Validate(studyFromDb);

            await _db.SaveChangesAsync();

            return await GetStudyByIdAsync(studyFromDb.Id);
        }

        void PerformUsualTestsForPostedStudy(int studyId, StudyDto updatedStudy)
        {
            if (studyId <= 0)
            {
                throw new ArgumentException("Id was zero or negative:" + studyId);
            }

            if (studyId != updatedStudy.Id)
            {
                throw new ArgumentException($"Id in url ({studyId}) is different from Id in data ({updatedStudy.Id})");
            }
        }

        // TODO: Deletion may be changed later to keep database entry, but remove from listing.
        public async Task<IEnumerable<StudyListItemDto>> DeleteStudyAsync(int studyId)
        {
            //TODO: VALIDATION
            //Delete logo from Azure Blob Storage before deleting study.
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);
            string logoUrl = studyFromDb.LogoUrl;
            if (!String.IsNullOrWhiteSpace(logoUrl))
            {            
                _ = _azureBlobStorageService.DeleteBlob(logoUrl);
            }

            //Check if study contains studySpecific Datasets
            List<Dataset> studySpecificDatasets = await _db.Datasets.Where(ds => ds.StudyNo == studyId).ToListAsync();
            if (studySpecificDatasets.Any())
            {
                foreach (Dataset dataset in studySpecificDatasets)
                {
                    // TODO: Possibly keep datasets for archiving/logging purposes.
                    // Possibly: Datasets.removeWithoutDeleting(dataset)
                    _db.Datasets.Remove(dataset);
                }
            }

            //Delete study
            // TODO: Possibly keep study for archiving/logging purposes.
            // Possibly: Studies.removeWithoutDeleting(study) Mark as deleted but keep record?
            _db.Studies.Remove(studyFromDb);
            await _db.SaveChangesAsync();
            return await GetStudiesAsync();
        }

        public async Task<StudyDto> AddLogoAsync(int studyId, IFormFile studyLogo)
        {        
            var fileName = _azureBlobStorageService.UploadBlob(studyLogo);
            var studyFromDb = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);
            string oldFileName = studyFromDb.LogoUrl;

            if (!String.IsNullOrWhiteSpace(fileName) && oldFileName != fileName)
            {
                studyFromDb.LogoUrl = fileName;
            }

            Validate(studyFromDb);
            await _db.SaveChangesAsync();

            if (!String.IsNullOrWhiteSpace(oldFileName))
            {
            _ = _azureBlobStorageService.DeleteBlob(oldFileName);
            }

            return await GetStudyByIdAsync(studyFromDb.Id);
        }

        public async Task<byte[]> GetLogoAsync(int studyId)
        {      
            var study = await StudyQueries.GetStudyOrThrowAsync(studyId, _db);
            string logoUrl = study.LogoUrl;
            var logo = _azureBlobStorageService.GetImageFromBlobAsync(logoUrl);
            return await logo;
        }

        protected bool CanViewRestrictedStudies(ClaimsPrincipal user)
        {
            //TODO: Open up for more than admins
            //TODO: Add relevant study specific roles

            if (user.IsInRole(Roles.Admin))
            {
                //TODO: Should really be true?
                return true;
            }

            //Do you have a participant role for this study? Return true


            return user.IsInRole(Roles.Admin);
        }
    }
}
