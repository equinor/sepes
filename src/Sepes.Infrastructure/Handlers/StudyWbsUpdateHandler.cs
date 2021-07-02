using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Constants.CloudResource;
using Sepes.Common.Dto.Sandbox;
using Sepes.Infrastructure.Handlers.Interface;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers
{
    public class StudyWbsUpdateHandler : IStudyWbsUpdateHandler
    {
        readonly ILogger _logger;
        readonly IConfiguration _configuration;
        readonly SepesDbContext _sepesDbContext;
        readonly IProvisioningQueueService _provisioningQueueService;
        readonly ICloudResourceOperationCreateService _cloudResourceOperationCreateService;

        public StudyWbsUpdateHandler(ILogger<StudyWbsUpdateHandler> logger, IConfiguration configuration, SepesDbContext sepesDbContext, IProvisioningQueueService provisioningQueueService, ICloudResourceOperationCreateService cloudResourceOperationCreateService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _sepesDbContext = sepesDbContext ?? throw new ArgumentNullException(nameof(sepesDbContext));
            _provisioningQueueService = provisioningQueueService ?? throw new ArgumentNullException(nameof(provisioningQueueService));
            _cloudResourceOperationCreateService = cloudResourceOperationCreateService ?? throw new ArgumentNullException(nameof(cloudResourceOperationCreateService));
        }

        public async Task Handle(Study study, CancellationToken cancellationToken = default)
        {
           
            var studyWithUser = await _sepesDbContext.Studies
                .Include(s => s.StudyParticipants)
                    .ThenInclude(sp => sp.User)
                    .SingleOrDefaultAsync(s => s.Id == study.Id);


            await UpdateTagsForStudySpecificDatasetsAsync(studyWithUser, cancellationToken);  

            await UpdateTagsForSandboxResourcesAsync(studyWithUser, cancellationToken);
        
        }

        public async Task UpdateTagsForStudySpecificDatasetsAsync(Study study, CancellationToken cancellationToken = default)
        {
            var parentQueueItem = QueueItemFactory.CreateParent("Update tags for Study specific dataset resources");

            _logger.LogInformation($"Updating tags for study specific dataset resources for study {study.Id}");

            var datasetResourceGroup = await GetStudySpecificDatasetResourceGroup(study);

            _logger.LogInformation($"Updating tags for study specific dataset container {datasetResourceGroup.Id}: {datasetResourceGroup.ResourceName}");

            var resourceGroupTags = ResourceTagFactory.StudySpecificDatasourceResourceGroupTags(_configuration, study);
            datasetResourceGroup.Tags = TagUtils.TagDictionaryToString(resourceGroupTags);

            await _sepesDbContext.SaveChangesAsync(cancellationToken);

            await CreateUpdateOperationAndEnqueue(parentQueueItem, datasetResourceGroup);

            //Update all datasets in resource group
            var datasetStorageAccounts = await GetStudySpecificDatasetStorageAccountResources(datasetResourceGroup);

            foreach (var curStorageAccount in datasetStorageAccounts)
            {
                _logger.LogInformation($"Updating tags for study specific dataset storage account {curStorageAccount.Id}: {curStorageAccount.ResourceName}");
                var tagsForStorageAccount = ResourceTagFactory.StudySpecificDatasourceStorageAccountTags(_configuration, study, curStorageAccount.Dataset.Name);
                curStorageAccount.Tags = TagUtils.TagDictionaryToString(tagsForStorageAccount);
                await CreateUpdateOperationAndEnqueue(parentQueueItem, curStorageAccount);
            }

            await _sepesDbContext.SaveChangesAsync(cancellationToken);

            await _provisioningQueueService.SendMessageAsync(parentQueueItem, cancellationToken: cancellationToken);

            _logger.LogInformation($"Done updating tags for study specific dataset resources for study {study.Id}");
        }

        async Task<CloudResource> GetStudySpecificDatasetResourceGroup(Study study)
        {
            return await _sepesDbContext.CloudResources.SingleOrDefaultAsync(r => r.StudyId == study.Id && !r.Deleted && r.Purpose == CloudResourcePurpose.StudySpecificDatasetContainer);
        }

        async Task<List<CloudResource>> GetStudySpecificDatasetStorageAccountResources(CloudResource parentResourceGroup)
        {
            return await _sepesDbContext.CloudResources.Include(r => r.Dataset).Where(r => r.ParentResourceId == parentResourceGroup.Id && !r.Deleted && r.Purpose == CloudResourcePurpose.StudySpecificDatasetStorageAccount).ToListAsync();
        }

        async Task CreateUpdateOperationAndEnqueue(ProvisioningQueueParentDto parentQueueItem, CloudResource resource)
        {
            var roleAssignmentUpdateOperation = await _cloudResourceOperationCreateService.CreateUpdateOperationAsync(resource.Id, CloudResourceOperationType.ENSURE_TAGS);
            ProvisioningQueueUtil.CreateChildAndAdd(parentQueueItem, roleAssignmentUpdateOperation);
        }

        async Task UpdateTagsForSandboxResourcesAsync(Study study, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Updating tags for sandbox resources for study {study.Id}");

            var sandboxes = await GetSandboxWithResourcesAsync(study, cancellationToken);

            foreach (var curSandbox in sandboxes)
            {
                var parentQueueItem = QueueItemFactory.CreateParent($"Update tags for Sandbox resources, Sandbox: {curSandbox.Id}");

                _logger.LogInformation($"Updating sandbox resource tags for {curSandbox.Id}: {curSandbox.Name}");

                var tag = ResourceTagFactory.SandboxResourceTags(_configuration, study, curSandbox);

                foreach (var curSandboxResource in curSandbox.Resources)
                {
                    _logger.LogInformation($"Updating tags for resource {curSandboxResource.Id}: {curSandboxResource.ResourceName}");
                    curSandboxResource.Tags = TagUtils.TagDictionaryToString(tag);
                    await CreateUpdateOperationAndEnqueue(parentQueueItem, curSandboxResource);
                }

                await _sepesDbContext.SaveChangesAsync(cancellationToken);
                await _provisioningQueueService.SendMessageAsync(parentQueueItem, cancellationToken: cancellationToken);
            }

            _logger.LogInformation($"Done updating tags for sandbox resources for study {study.Id}");
        }

        async Task<List<Sandbox>> GetSandboxWithResourcesAsync(Study study, CancellationToken cancellationToken)
        {
            return await _sepesDbContext.Sandboxes.Include(sb => sb.Resources).Where(sb => sb.StudyId == study.Id && !sb.Deleted).ToListAsync(cancellationToken);
        }
    }
}
