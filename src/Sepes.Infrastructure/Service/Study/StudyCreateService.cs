using AutoMapper;
using Microsoft.AspNetCore.Http;
using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Common.Dto.Study;
using Sepes.Common.Util;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class StudyCreateService : IStudyCreateService
    {
        readonly SepesDbContext _db;
        readonly IMapper _mapper;
        readonly IUserService _userService;

        readonly IStudyEfModelService _studyModelService;
        readonly IOperationPermissionService _operationPermissionService;
        readonly IStudyLogoCreateService _studyLogoCreateService;
        readonly IDatasetCloudResourceService _datasetCloudResourceService;
        readonly IStudyWbsValidationService _studyWbsValidationService;


        public StudyCreateService(SepesDbContext db,
            IMapper mapper,
            IUserService userService,
            IStudyEfModelService studyModelService,
            IStudyLogoCreateService studyLogoCreateService,
            IOperationPermissionService operationPermissionService,
            IDatasetCloudResourceService datasetCloudResourceService,
            IStudyWbsValidationService studyWbsValidationService
           )
        {
            _db = db;
            _userService = userService;
            _mapper = mapper;

            _studyModelService = studyModelService;

            _operationPermissionService = operationPermissionService;
            _studyLogoCreateService = studyLogoCreateService;
            _datasetCloudResourceService = datasetCloudResourceService;
            _studyWbsValidationService = studyWbsValidationService;
        }

        public async Task<Study> CreateAsync(StudyCreateDto newStudyDto, IFormFile logo = null, CancellationToken cancellation = default)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            await _operationPermissionService.HasAccessToOperationOrThrow(UserOperation.Study_Create);
            GenericNameValidation.ValidateName(newStudyDto.Name);

            var studyDb = _mapper.Map<Study>(newStudyDto);

            MakeCurrentUserOwnerOfStudy(studyDb, currentUser);

            await _studyWbsValidationService.ValidateForStudyCreate(studyDb);

            studyDb = await _studyModelService.AddAsync(studyDb);

            await _datasetCloudResourceService.CreateResourceGroupForStudySpecificDatasetsAsync(studyDb, cancellation);

            if (logo != null)
            {
                studyDb.LogoUrl = await _studyLogoCreateService.CreateAsync(studyDb, logo);
                await _db.SaveChangesAsync();
            }

            return studyDb;
        }

        void MakeCurrentUserOwnerOfStudy(Study study, UserDto user)
        {
            study.StudyParticipants = new List<StudyParticipant>
            {
                new StudyParticipant() { UserId = user.Id, RoleName = StudyRoles.StudyOwner, Created = DateTime.UtcNow, CreatedBy = user.UserName }
            };
        }
    }
}
