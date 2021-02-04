﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Dto.Study;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using Sepes.Infrastructure.Util.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Sepes.Infrastructure.Service
{
    public class StudyCreateUpdateService : StudyServiceBase, IStudyCreateUpdateService
    {  
        public StudyCreateUpdateService(SepesDbContext db, IMapper mapper, ILogger<StudyCreateUpdateService> logger, IUserService userService, IStudyModelService studyModelService, IStudyLogoService studyLogoService)
            : base(db, mapper, logger, userService, studyModelService, studyLogoService)
        {
      
         
        }      

        public async Task<StudyDetailsDto> CreateStudyAsync(StudyCreateDto newStudyDto)
        {
            GenericNameValidation.ValidateName(newStudyDto.Name);
            StudyAccessUtil.HasAccessToOperationOrThrow(await _userService.GetCurrentUserWithStudyParticipantsAsync(), UserOperation.Study_Create);

            var studyDb = _mapper.Map<Study>(newStudyDto);

            var currentUser = await _userService.GetCurrentUserAsync();
            MakeCurrentUserOwnerOfStudy(studyDb, currentUser);

            var newStudyId = await Add(studyDb);
            return await GetStudyDetailsDtoByIdAsync(newStudyId, UserOperation.Study_Create);
        }

        public async Task<StudyDetailsDto> UpdateStudyMetadataAsync(int studyId, StudyDto updatedStudy)
        {
            GenericNameValidation.ValidateName(updatedStudy.Name);

            var studyFromDb = await GetStudyByIdAsync(studyId, UserOperation.Study_Update_Metadata, false);

            PerformUsualTestsForPostedStudy(studyId, updatedStudy);

            if (updatedStudy.Name != studyFromDb.Name)
            {
                studyFromDb.Name = updatedStudy.Name;
            }

            if (updatedStudy.Description != studyFromDb.Description)
            {
                studyFromDb.Description = updatedStudy.Description;
            }

            if (updatedStudy.Vendor != studyFromDb.Vendor)
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

            studyFromDb.Updated = DateTime.UtcNow;

            Validate(studyFromDb);

            await _db.SaveChangesAsync();

            return await GetStudyDetailsDtoByIdAsync(studyFromDb.Id, UserOperation.Study_Update_Metadata);
        }

      

        public async Task<StudyResultsAndLearningsDto> UpdateResultsAndLearningsAsync(int studyId, StudyResultsAndLearningsDto resultsAndLearnings)
        {
            var studyFromDb = await GetStudyByIdAsync(studyId, UserOperation.Study_Update_ResultsAndLearnings, false);

            if (resultsAndLearnings.ResultsAndLearnings != studyFromDb.ResultsAndLearnings)
            {
                studyFromDb.ResultsAndLearnings = resultsAndLearnings.ResultsAndLearnings;
            }

            var currentUser = await _userService.GetCurrentUserAsync();
            studyFromDb.Updated = DateTime.UtcNow;
            studyFromDb.UpdatedBy = currentUser.UserName;

            await _db.SaveChangesAsync();

            return new StudyResultsAndLearningsDto() { ResultsAndLearnings = studyFromDb.ResultsAndLearnings };
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

        void MakeCurrentUserOwnerOfStudy(Study study, UserDto user)
        {
            study.StudyParticipants = new List<StudyParticipant>();
            study.StudyParticipants.Add(new StudyParticipant() { UserId = user.Id, RoleName = StudyRoles.StudyOwner, Created = DateTime.UtcNow, CreatedBy = user.UserName });
        }
    }
}
