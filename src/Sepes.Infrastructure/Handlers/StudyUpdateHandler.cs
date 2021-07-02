using Microsoft.AspNetCore.Http;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Study;
using Sepes.Common.Util;
using Sepes.Infrastructure.Handlers.Interface;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Service.Queries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers
{
    public class StudyUpdateHandler : IStudyUpdateHandler
    {
        readonly SepesDbContext _db;
        readonly IUserService _userService;
        readonly IStudyLogoCreateService _studyLogoCreateService;
        readonly IStudyLogoDeleteService _studyLogoDeleteService;
        readonly IStudyWbsValidationService _studyWbsValidationService;
        readonly IStudyWbsUpdateHandler _studyWbsUpdateHandler;

        readonly IStudyEfModelOperationsService _studyEfModelOperationsService;

        public StudyUpdateHandler(SepesDbContext db,
            IUserService userService,
            IStudyLogoCreateService studyLogoCreateService,
            IStudyLogoDeleteService studyLogoDeleteService,
            IStudyWbsValidationService studyWbsValidationService,
            IStudyWbsUpdateHandler studyWbsUpdateHandler,
            IStudyEfModelOperationsService studyEfModelOperationsService
          )
        {
            _db = db;
            _userService = userService;
            _studyLogoCreateService = studyLogoCreateService;
            _studyLogoDeleteService = studyLogoDeleteService;
            _studyWbsValidationService = studyWbsValidationService;

            _studyWbsUpdateHandler = studyWbsUpdateHandler;
            _studyEfModelOperationsService = studyEfModelOperationsService;
        }

        public async Task<Study> UpdateAsync(int studyId, StudyUpdateDto updatedStudy, IFormFile logo = null, CancellationToken cancellationToken = default)
        {
            var spUpdate = Stopwatch.StartNew();

            if (studyId <= 0)
            {
                throw new ArgumentException("Study Id was zero or negative:" + studyId);
            }

            GenericNameValidation.ValidateName(updatedStudy.Name);

            var studyFromDb = await GetStudyAsync(studyId);       

            var TasksToWaitFor = new List<Task>
            {
                HandleWbsChanged(updatedStudy, studyFromDb, cancellationToken),

                HandleLogoChanged(updatedStudy, studyFromDb, logo, cancellationToken),

                HandleSimpleValuesChanged(updatedStudy, studyFromDb)
            };

            await Task.WhenAll(TasksToWaitFor);

            EntityValidationUtil.Validate<Study>(studyFromDb);           

            await _db.SaveChangesAsync();          

            return studyFromDb;
        }

        async Task HandleWbsChanged(StudyUpdateDto studyFromClient, Study studyFromDb, CancellationToken cancellationToken)
        {           
            if (studyFromClient.WbsCode != studyFromDb.WbsCode)
            {
                studyFromDb.WbsCode = studyFromClient.WbsCode;             

                await _studyWbsValidationService.ValidateForStudyUpdate(studyFromDb); 
                await _studyWbsUpdateHandler.Handle(studyFromDb, cancellationToken); 
            }
        }

        async Task HandleLogoChanged(StudyUpdateDto studyFromClient, Study studyFromDb, IFormFile logo = null, CancellationToken cancellationToken = default)
        {
            if (studyFromClient.DeleteLogo)
            {
                if (!String.IsNullOrWhiteSpace(studyFromDb.LogoUrl))
                {
                    await _studyLogoDeleteService.DeleteAsync(studyFromDb.LogoUrl);
                    studyFromDb.LogoUrl = "";
                }
            }
            else if (logo != null)
            {
                studyFromDb.LogoUrl = await _studyLogoCreateService.CreateAsync(studyFromDb, logo);
            }
        }

        async Task HandleSimpleValuesChanged(StudyUpdateDto studyFromClient, Study studyFromDb)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            studyFromDb.Updated = DateTime.UtcNow;
            studyFromDb.UpdatedBy = currentUser.UserName;

            if (studyFromClient.Name != studyFromDb.Name)
            {
                studyFromDb.Name = studyFromClient.Name;
            }

            if (studyFromClient.Description != studyFromDb.Description)
            {
                studyFromDb.Description = studyFromClient.Description;
            }

            if (studyFromClient.Vendor != studyFromDb.Vendor)
            {
                studyFromDb.Vendor = studyFromClient.Vendor;
            }

            if (studyFromClient.Restricted != studyFromDb.Restricted)
            {
                studyFromDb.Restricted = studyFromClient.Restricted;
            }
        }

        public async Task<Study> GetStudyAsync(int studyId)
        {
            var queryable = StudyBaseQueries.ActiveStudiesWithParticipantsQueryable(_db);
            return await _studyEfModelOperationsService.GetStudyFromQueryableThrowIfNotFoundOrNoAccess(queryable, studyId, UserOperation.Study_Update_Metadata);
        }
    }
}
