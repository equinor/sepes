using AutoMapper;
using Sepes.Common.Dto.Dataset;
using Sepes.Common.Dto.Study;
using Sepes.Common.Interface;
using Sepes.Common.Response.Sandbox;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class StudyDetailsService : IStudyDetailsService
    {
        readonly IMapper _mapper;
        readonly IUserService _userService;
        readonly IStudyLogoReadService _studyLogoReadService;
        readonly IStudyPermissionService _studyPermissionService;
        readonly IStudyDetailsModelService _studyDetailsModelService;
 

        public StudyDetailsService(IMapper mapper, IUserService userService, IStudyLogoReadService studyLogoReadService, IStudyPermissionService studyPermissionService, IStudyDetailsModelService studyDetailsModelService)         
        {
            _mapper = mapper;
            _userService = userService;
            _studyLogoReadService = studyLogoReadService;
            _studyPermissionService = studyPermissionService;
            _studyDetailsModelService = studyDetailsModelService;
        }      

        public async Task<StudyDetailsDto> Get(int studyId)
        {
            var spUpdate = Stopwatch.StartNew();

            var studyFromDbDapper = await _studyDetailsModelService.GetStudyDetailsAsync(studyId);
            var studyDetailsDto = _mapper.Map<StudyDetailsDto>(studyFromDbDapper);

            var afterStudy = spUpdate.ElapsedMilliseconds;
            spUpdate.Restart();

            var sandboxes = await _studyDetailsModelService.GetSandboxForStudyDetailsAsync(studyId);
            studyDetailsDto.Sandboxes = _mapper.Map<List<SandboxListItem>>(sandboxes);

            var aftersb = spUpdate.ElapsedMilliseconds;
            spUpdate.Restart();

            var datasets = await _studyDetailsModelService.GetDatasetsForStudyDetailsAsync(studyId);
            studyDetailsDto.Datasets = _mapper.Map<List<DatasetListItemDto>>(datasets);

            var afterds = spUpdate.ElapsedMilliseconds;
            spUpdate.Restart();

            var participants = await _studyDetailsModelService.GetParticipantsForStudyDetailsAsync(studyId);
            studyDetailsDto.Participants = _mapper.Map<List<StudyParticipantListItem>>(participants);

            var afterParticipants = spUpdate.ElapsedMilliseconds;
            spUpdate.Restart();

            await _studyLogoReadService.DecorateLogoUrlWithSAS(studyDetailsDto);

            var afterLogo = spUpdate.ElapsedMilliseconds;
            spUpdate.Restart();

            await DecorateStudyWithPermissions(studyDetailsDto);

            var afterPermissions = spUpdate.ElapsedMilliseconds;
            spUpdate.Restart();

            return studyDetailsDto;
        }

        public async Task DecorateStudyWithPermissions(StudyDetailsDto dto)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var studyPermissionDetails = _mapper.Map<IHasStudyPermissionDetails>(dto);

            dto.Permissions.UpdateMetadata = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_Update_Metadata);
            dto.Permissions.CloseStudy = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_Close);
            dto.Permissions.DeleteStudy = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_Close);

            dto.Permissions.ReadResulsAndLearnings = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_Read_ResultsAndLearnings);
            dto.Permissions.UpdateResulsAndLearnings = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_Update_ResultsAndLearnings);

            dto.Permissions.AddRemoveDataset = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_AddRemove_Dataset);
            dto.Permissions.AddRemoveSandbox = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_Crud_Sandbox);
            dto.Permissions.AddRemoveParticipant = _studyPermissionService.HasAccessToOperationForStudy(currentUser, studyPermissionDetails, Common.Constants.UserOperation.Study_AddRemove_Participant);
        }
    }
}
