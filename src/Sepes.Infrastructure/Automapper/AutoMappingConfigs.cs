using AutoMapper;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using Sepes.Azure.Dto;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Common.Dto.Dataset;
using Sepes.Common.Dto.Sandbox;
using Sepes.Common.Dto.Study;
using Sepes.Common.Dto.VirtualMachine;
using Sepes.Common.Interface;
using Sepes.Common.Response.Sandbox;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using System.Linq;

namespace Sepes.Infrastructure.Automapper
{
    public class AutoMappingConfigs : Profile
    {
        public AutoMappingConfigs()
        {
            //STUDY
            CreateMap<Study, StudyDto>()
                .ForMember(dest => dest.Participants, source => source.MapFrom(x => x.StudyParticipants));

            CreateMap<Study, StudyDetailsDto>()
                .ForMember(dest => dest.Datasets, source => source.MapFrom(x => x.StudyDatasets.Select(y => y.Dataset).Where(d => !d.Deleted).ToList()))
                .ForMember(dest => dest.Sandboxes, source => source.MapFrom(x => x.Sandboxes.Where(sb => !sb.Deleted).ToList()))
                .ForMember(dest => dest.Participants, source => source.MapFrom(x => x.StudyParticipants));

            CreateMap<StudyDetailsDapper, StudyDetailsDto>()
                      .ForMember(dest => dest.Id, source => source.MapFrom(x => x.StudyId));
            CreateMap<Study, StudyListItemDto>();         
            CreateMap<StudyDto, Study>();
            CreateMap<StudyCreateDto, Study>();

            CreateMap<Study, IHasStudyPermissionDetails>()
                 .ForMember(dest => dest.StudyId, source => source.MapFrom(x => x.Id))
                  .ForMember(dest => dest.Restricted, source => source.MapFrom(x => x.Restricted))
                   .ForMember(dest => dest.UsersAndRoles, source => source.MapFrom<StudyToStudyPermissionDetailsResolver>());

            CreateMap<StudyDetailsDto, IHasStudyPermissionDetails>()
               .ForMember(dest => dest.StudyId, source => source.MapFrom(x => x.Id))
                .ForMember(dest => dest.Restricted, source => source.MapFrom(x => x.Restricted))
                 .ForMember(dest => dest.UsersAndRoles, source => source.MapFrom<StudyDetailsToStudyPermissionDetailsResolver>());

            //DATASET

            CreateMap<DatasetCreateUpdateInputBaseDto, Dataset>();

            CreateMap<Dataset, DatasetDto>()
                .ForMember(dest => dest.Studies,source => source.MapFrom(x => x.StudyDatasets.Select(y => y.Study).ToList()))
                .ForMember(dest => dest.StorageAccountName, source => source.MapFrom<DatasetStorageAccountNameResolver>())
                .ForMember(dest => dest.StorageAccountLink, source => source.MapFrom<StorageAccountExternalLinkResolver>());
            
            CreateMap<Dataset, DatasetLookupItemDto>();

            CreateMap<DatasetForStudyDetailsDapper, DatasetListItemDto>()
                   .ForMember(dest => dest.Id, source => source.MapFrom(x => x.DatasetId))
                   .ForMember(dest => dest.Name, source => source.MapFrom(x => x.DatasetName))
            .ForMember(dest => dest.Sandboxes, source => source.MapFrom(x => x.Sandboxes));

            CreateMap<Dataset, DatasetListItemDto>()
                         .ForMember(dest => dest.StudyId, source => source.MapFrom(ds => ds.StudySpecific ? ds.StudyDatasets.SingleOrDefault().StudyId : default(int?)))
                     .ForMember(dest => dest.Sandboxes, source => source.MapFrom(x => x.SandboxDatasets.Where(sd => !sd.Sandbox.Deleted).Select(sd => sd.Sandbox).ToList()));

            CreateMap<StudyDataset, StudyDatasetDto>()
                  .ForMember(dest => dest.Id, source => source.MapFrom(x => x.Dataset.Id))
                        .ForMember(dest => dest.Name, source => source.MapFrom(x => x.Dataset.Name))
                .ForMember(dest => dest.DataId, source => source.MapFrom(x => x.Dataset.DataId))
                .ForMember(dest => dest.Classification, source => source.MapFrom(x => x.Dataset.Classification));

            CreateMap<SandboxDataset, SandboxDatasetDto>()
                .ForMember(dest => dest.DatasetId, source => source.MapFrom(x => x.Dataset.Id))
                .ForMember(dest => dest.Name, source => source.MapFrom(x => x.Dataset.Name))
                .ForMember(dest => dest.Classification, source => source.MapFrom(x => x.Dataset.Classification))
                .ForMember(dest => dest.SandboxName, opt =>
                {
                    opt.PreCondition(src => (!src.Sandbox.Deleted));
                    opt.MapFrom(src =>

                        src.Sandbox.Name
                    );
                })
                .ForMember(dest => dest.SandboxId, source => source.MapFrom(x => x.Sandbox.Id))
                .ForMember(dest => dest.StudyId, source => source.MapFrom(x => x.Sandbox.StudyId));

            CreateMap<CloudResource, DatasetResourceLightDto>()
                 .ForMember(dest => dest.Name, source => source.MapFrom(x => x.ResourceName))
                  .ForMember(dest => dest.Type, source => source.MapFrom(x => AzureResourceTypeUtil.GetUserFriendlyName(x.ResourceType)))
                   .ForMember(dest => dest.Status, source => source.MapFrom(x => ResourceStatusUtil.ResourceStatus(x)))
                     .ForMember(dest => dest.LinkToExternalSystem, source => source.MapFrom<StorageAccountResourceExternalLinkResolver>())
                     .ForMember(dest => dest.RetryLink, source => source.MapFrom<DatasetResourceRetryLinkResolver>());
                      

            //SANDBOX
            CreateMap<Sandbox, SandboxDto>()
                 .ForMember(dest => dest.StudyName, source => source.MapFrom(x => x.Study.Name))
                  .ForMember(dest => dest.CurrentPhase, source => source.MapFrom<SandboxPhaseNameResolver>());


            CreateMap<Sandbox, SandboxListItem>();

            CreateMap<SandboxForStudyDetailsDapper, SandboxListItem>()
                 .ForMember(dest => dest.Id, source => source.MapFrom(x => x.SandboxId))
                   .ForMember(dest => dest.Name, source => source.MapFrom(x => x.SandboxName))
                   .ForMember(dest => dest.StudyId, source => source.MapFrom(x => x.StudyId));

            CreateMap<Sandbox, SandboxDetails>()       
         .ForMember(dest => dest.StudyName, source => source.MapFrom(x => x.Study.Name))
         .ForMember(dest => dest.Datasets, source => source.MapFrom(x => x.SandboxDatasets))
         .ForMember(dest => dest.LinkToCostAnalysis, source => source.MapFrom<SandboxResourceExternalCostAnalysis>())
           .ForMember(dest => dest.CurrentPhase, source => source.MapFrom<SandboxPhaseNameResolver>());         

            CreateMap<SandboxCreateDto, Sandbox>();

            CreateMap<CloudResource, SandboxResourceLight>()
            .ForMember(dest => dest.Name, source => source.MapFrom(x => x.ResourceName))           
             .ForMember(dest => dest.Type, source => source.MapFrom(x => AzureResourceTypeUtil.GetUserFriendlyName(x.ResourceType)))
              .ForMember(dest => dest.Status, source => source.MapFrom(x => ResourceStatusUtil.ResourceStatus(x)))
                .ForMember(dest => dest.LinkToExternalSystem, source => source.MapFrom<SandboxResourceExternalLinkResolver>())
                .ForMember(dest => dest.RetryLink, source => source.MapFrom<SandboxResourceRetryLinkResolver>())
                  .ForMember(dest => dest.AdditionalProperties, source => source.MapFrom<SandboxResourceAdditionalPropertiesResolver>())
                ;


            //CLOUD RESOURCE

            CreateMap<CloudResource, CloudResourceDto>()
                .ForMember(dest => dest.Tags, source => source.MapFrom(x => TagUtils.TagStringToDictionary(x.Tags)))
                .ForMember(dest => dest.SandboxName, source => source.MapFrom(s => s.Sandbox.Name))
            .ForMember(dest => dest.StudyName, source => source.MapFrom(s => s.Sandbox.Study.Name));


            CreateMap<CloudResourceDto, CloudResource>()
                .ForMember(dest => dest.Tags, source => source.MapFrom(x => TagUtils.TagDictionaryToString(x.Tags)));

            CreateMap<CloudResourceOperation, CloudResourceOperationDto>();
            CreateMap<CloudResourceOperationDto, CloudResourceOperation>();

            //USERS/PARTICIPANTS

            CreateMap<User, UserDto>().ReverseMap();

            CreateMap<User, ParticipantLookupDto>()
                    .ForMember(dest => dest.Source, source => source.MapFrom(s => ParticipantSource.Db))
                    .ForMember(dest => dest.DatabaseId, source => source.MapFrom(s => s.Id));

            CreateMap<Microsoft.Graph.User, AzureUserDto>();

            CreateMap<AzureUserDto, ParticipantLookupDto>()
                    .ForMember(dest => dest.ObjectId, source => source.MapFrom(x => x.Id))
                    .ForMember(dest => dest.FullName, source => source.MapFrom(x => x.DisplayName))
                    .ForMember(dest => dest.EmailAddress, source => source.MapFrom(x => x.Mail))                   
                    .ForMember(dest => dest.UserName, source => source.MapFrom(x => x.UserPrincipalName))
                    .ForMember(dest => dest.Source, source => source.MapFrom(s => ParticipantSource.Azure));

            CreateMap<StudyParticipant, StudyParticipantDto>()
                .ForMember(dest => dest.EmailAddress, source => source.MapFrom(x => x.User.EmailAddress))
                .ForMember(dest => dest.FullName, source => source.MapFrom(x => x.User.FullName))
                .ForMember(dest => dest.UserName, source => source.MapFrom(x => x.User.UserName))
                .ForMember(dest => dest.Role, source => source.MapFrom(x => x.RoleName));

            CreateMap<StudyParticipant, StudyParticipantListItem>()
              .ForMember(dest => dest.EmailAddress, source => source.MapFrom(x => x.User.EmailAddress))
              .ForMember(dest => dest.FullName, source => source.MapFrom(x => x.User.FullName))
              .ForMember(dest => dest.UserName, source => source.MapFrom(x => x.User.UserName))
              .ForMember(dest => dest.Role, source => source.MapFrom(x => x.RoleName));

            CreateMap<StudyParticipantForStudyDetailsDapper, StudyParticipantListItem>()  
            .ForMember(dest => dest.Role, source => source.MapFrom(x => x.Role));
            

            //AZURE
            CreateMap<IResource, AzureResourceDto>();

            CreateMap<IResourceGroup, AzureResourceGroupDto>()
                 .ForMember(dest => dest.ProvisioningState, source => source.MapFrom(x => x.ProvisioningState));

            CreateMap<IStorageAccount, AzureStorageAccountDto>()
                .ForMember(dest => dest.ProvisioningState, source => source.MapFrom(x => x.ProvisioningState));

            CreateMap<VirtualMachineCreateDto, VmSettingsDto>();

            CreateMap<CloudResourceDto, VmDto>()
                .ForMember(dest => dest.Name, source => source.MapFrom(x => x.ResourceName))
                 .ForMember(dest => dest.Region, source => source.MapFrom(x => RegionStringConverter.Convert(x.Region).Name));

            CreateMap<CloudResource, VmDto>()
           .ForMember(dest => dest.Name, source => source.MapFrom(x => x.ResourceName))
            .ForMember(dest => dest.Region, source => source.MapFrom(x => RegionStringConverter.Convert(x.Region).Name))
            .ForMember(dest => dest.Status, source => source.MapFrom(x => ResourceStatusUtil.ResourceStatus(x)))
            .ForMember(dest => dest.OperatingSystem, source => source.MapFrom(x => VmOsUtil.GetOsName(x)))
                   .ForMember(dest => dest.LinkToExternalSystem, source => source.MapFrom<SandboxResourceExternalLinkResolver>());


            CreateMap<VmRuleDto, NsgRuleDto>()
                      .ForMember(dest => dest.Protocol, source => source.MapFrom(x => x.Protocol))
                  .ForMember(dest => dest.Description, source => source.MapFrom(x => x.Description));


            CreateMap<AzureRegionDto, LookupDto>();

            CreateMap<VmSize, VmSizeDto>()
                  .ForMember(dest => dest.Name, source => source.MapFrom(x => x.Key));

            CreateMap<VmSize, VmSizeLookupDto>()
               .ForMember(dest => dest.DisplayValue, source => source.MapFrom(x => x.DisplayText));

              
        }
    }
}
