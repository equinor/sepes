﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Constants.CloudResource;
using Sepes.Infrastructure.Dto.Sandbox;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util;
using Sepes.Infrastructure.Util.Auth;
using Sepes.Infrastructure.Util.Provisioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class DatasetCloudResourceService : IDatasetCloudResourceService
    {
        readonly IConfiguration _config;
        readonly SepesDbContext _db;
        readonly ILogger<DatasetCloudResourceService> _logger;

        readonly IUserService _userService;

        readonly IStudyModelService _studyModelService;

        readonly ICloudResourceCreateService _cloudResourceCreateService;
        readonly ICloudResourceOperationCreateService _cloudResourceOperationCreateService;
        readonly IProvisioningQueueService _provisioningQueueService;

        public DatasetCloudResourceService(IConfiguration config, SepesDbContext db, ILogger<DatasetCloudResourceService> logger,
           IUserService userService,
           IStudyModelService studyModelService,
           ICloudResourceCreateService cloudResourceCreateService,
           ICloudResourceOperationCreateService cloudResourceOperationCreateService,
           IProvisioningQueueService provisioningQueueService)
        {
            _config = config;
            _db = db;
            _logger = logger;
            _userService = userService;
            _studyModelService = studyModelService;
            _cloudResourceCreateService = cloudResourceCreateService;        
            _cloudResourceOperationCreateService = cloudResourceOperationCreateService;
            _provisioningQueueService = provisioningQueueService;
        }

        public async Task CreateResourceGroupForStudySpecificDatasetsAsync(Study study, CancellationToken cancellationToken = default)
        {
            var studyForCreation = await _studyModelService.GetByIdWithoutPermissionCheckAsync(study.Id, true, false);
            var resourceGroupForDatasets = GetResourceGroupForStudySpecificDataset(studyForCreation);

            var parentQueueItem = QueueItemFactory.CreateParent("Create resource group for Study specific datasets");

            if (resourceGroupForDatasets == null)
            {
                var resourceGroupName = AzureResourceNameUtil.StudySpecificDatasetResourceGroup(studyForCreation.Name);
                var tags = AzureResourceTagsFactory.StudySpecificDatasourceResourceGroupTags(_config, studyForCreation);
                resourceGroupForDatasets = await _cloudResourceCreateService.CreateStudySpecificResourceGroupEntryAsync(studyForCreation.Id, resourceGroupName, "norwayeast", tags);
                ProvisioningQueueUtil.CreateChildAndAdd(parentQueueItem, resourceGroupForDatasets);
            }
            else
            {
                throw new Exception("Resource group allready exists");
            }

            await ScheduleResourceGroupRoleAssignments(studyForCreation, resourceGroupForDatasets, parentQueueItem);

            await _provisioningQueueService.SendMessageAsync(parentQueueItem, cancellationToken: cancellationToken);
        }

        public async Task CreateResourcesForStudySpecificDatasetAsync(Dataset dataset, string clientIp, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"CreateResourcesForStudySpecificDataset - Dataset Id: {dataset.Id}");

            var parentQueueItem = QueueItemFactory.CreateParent("Create resources for Study Specific Dataset");
            var resourceGroupDb = GetResourceGroupForStudySpecificDataset(dataset.Study);     
            await OrderCreationOfStudySpecificDatasetStorageAccount(dataset, resourceGroupDb, clientIp, parentQueueItem, cancellationToken);

            await _provisioningQueueService.SendMessageAsync(parentQueueItem, cancellationToken: cancellationToken);
        }      

        CloudResource GetResourceGroupForStudySpecificDataset(Study study, bool includeDeleted = false)
        {
            if (study.Resources == null)
            {
                throw new Exception("Missing Include for CloudResource on Study");
            }

            foreach (var curResource in study.Resources)
            {
                if (SoftDeleteUtil.IsMarkedAsDeleted(curResource) == false || includeDeleted)
                {
                    if (curResource.ResourceType == AzureResourceType.ResourceGroup)
                    {
                        if (String.IsNullOrWhiteSpace(curResource.Purpose) == false && curResource.Purpose == CloudResourcePurpose.StudySpecificDatasetContainer)
                        {
                            return curResource;
                        }
                    }
                }
            }

            return null;
        }

        async Task ScheduleResourceGroupRoleAssignments(Study study, CloudResource resourceGroup, ProvisioningQueueParentDto queueParentItem)
        {
            var participants = await _db.StudyParticipants.Include(sp => sp.User).Where(p => p.StudyId == study.Id).ToListAsync();
            var desiredRoles = ParticipantRoleToAzureRoleTranslator.CreateDesiredRolesForStudyResourceGroup(participants);
            var desiredRolesSerialized = CloudResourceConfigStringSerializer.Serialize(desiredRoles);

            var resourceGroupCreateOperation = CloudResourceOperationUtil.GetCreateOperation(resourceGroup);

            var roleAssignmentUpdateOperation = await _cloudResourceOperationCreateService.CreateUpdateOperationAsync(resourceGroup.Id, CloudResourceOperationType.ENSURE_ROLES, dependsOn: resourceGroupCreateOperation.Id, desiredState: desiredRolesSerialized);

            ProvisioningQueueUtil.CreateChildAndAdd(queueParentItem, roleAssignmentUpdateOperation);
        }

        async Task OrderCreationOfStudySpecificDatasetStorageAccount(Dataset dataset, CloudResource resourceGroup, string clientIp, ProvisioningQueueParentDto queueParent, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"CreateResourcesForStudySpecificDataset - Dataset Id: {dataset.Id}");

                var currentUser = await _userService.GetCurrentUserAsync();

                var tagsForStorageAccount = AzureResourceTagsFactory.StudySpecificDatasourceStorageAccountTags(_config, dataset.Study, dataset.Name);
                var storageAccountName = AzureResourceNameUtil.StudySpecificDataSetStorageAccount(dataset.Name);
                               
                var resourceEntry = await _cloudResourceCreateService.CreateStudySpecificDatasetEntryAsync(dataset.Id, resourceGroup.Id, resourceGroup.Region, resourceGroup.ResourceGroupName, storageAccountName, tagsForStorageAccount);

                ProvisioningQueueUtil.CreateChildAndAdd(queueParent, resourceEntry);

                await DatasetFirewallUtils.SetDatasetFirewallRules(_config, _logger, currentUser, dataset, clientIp);

                await _db.SaveChangesAsync();

                var stateForFirewallOperation = DatasetFirewallUtils.TranslateAllowedIpsToOperationDesiredState(dataset.FirewallRules.ToList());

                var createStorageAccountOperation = CloudResourceOperationUtil.GetCreateOperation(resourceEntry);
                var firewallUpdateOperation = await _cloudResourceOperationCreateService.CreateUpdateOperationAsync(resourceEntry.Id, CloudResourceOperationType.ENSURE_FIREWALL_RULES, dependsOn: createStorageAccountOperation.Id, desiredState: stateForFirewallOperation);

                ProvisioningQueueUtil.CreateChildAndAdd(queueParent, firewallUpdateOperation);

                var stateForCorsRules = DatasetCorsUtils.CreateDatasetCorsRules(_config);
                var corsUpdateOperation = await _cloudResourceOperationCreateService.CreateUpdateOperationAsync(resourceEntry.Id, CloudResourceOperationType.ENSURE_CORS_RULES, dependsOn: firewallUpdateOperation.Id, desiredState: stateForCorsRules);

                ProvisioningQueueUtil.CreateChildAndAdd(queueParent, corsUpdateOperation);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to schedule creation of Azure Storage Account", ex);
            }
        }

        public async Task EnsureExistFirewallExceptionForApplication(Study study, Dataset dataset, CancellationToken cancellationToken = default)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var serverRule = await DatasetFirewallUtils.CreateServerRuleAsync(_config, currentUser);

            bool serverRuleAllreadyExist = false;

            var rulesToRemove = new List<DatasetFirewallRule>();

            //Get firewall rules from  database, determine of some have to be deleted
            foreach (var curDbRule in dataset.FirewallRules)
            {
                if (curDbRule.RuleType == DatasetFirewallRuleType.Api)
                {
                    if (curDbRule.Address == serverRule.Address)
                    {
                        serverRuleAllreadyExist = true;
                    }
                    else
                    {
                        if (curDbRule.Created.AddMonths(1) < DateTime.UtcNow)
                        {
                            rulesToRemove.Add(curDbRule);
                        }
                    }
                }
            }

            foreach (var curRuleToRemove in rulesToRemove)
            {
                dataset.FirewallRules.Remove(curRuleToRemove);
            }

            if (!serverRuleAllreadyExist)
            {
                dataset.FirewallRules.Add(serverRule);
            }

            await _db.SaveChangesAsync(cancellationToken);

            var stateForFirewallOperation = DatasetFirewallUtils.TranslateAllowedIpsToOperationDesiredState(dataset.FirewallRules.ToList());
            var datasetStorageAccountResource = DatasetUtils.GetStudySpecificStorageAccountResourceEntry(dataset);
            var firewallUpdateOperation = await _cloudResourceOperationCreateService.CreateUpdateOperationAsync(datasetStorageAccountResource.Id,
                CloudResourceOperationType.ENSURE_FIREWALL_RULES, desiredState: stateForFirewallOperation);

            await ProvisioningQueueUtil.CreateItemAndEnqueue(_provisioningQueueService, firewallUpdateOperation);
        }

        public async Task DeleteAllStudyRelatedResourcesAsync(Study study, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"DeleteAllStudyRelatedResourcesAsync - Study Id: {study.Id}");

            try
            {
                var resourceGroupEntry = GetResourceGroupForStudySpecificDataset(study, true);

                if (resourceGroupEntry != null)
                {
                    var currentUser = await _userService.GetCurrentUserAsync();

                    SoftDeleteUtil.MarkAsDeleted(resourceGroupEntry, currentUser);

                    foreach (var curResource in resourceGroupEntry.ChildResources)
                    {
                        SoftDeleteUtil.MarkAsDeleted(curResource, currentUser);
                    }

                    var deleteOperation = await _cloudResourceOperationCreateService.CreateDeleteOperationAsync(resourceGroupEntry.Id, $"Delete study related resurces for Study {study.Id}");
                    await ProvisioningQueueUtil.CreateItemAndEnqueue(_provisioningQueueService, deleteOperation);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete resources for study specific dataset", ex);
            }
        }

        public async Task DeleteResourcesForStudySpecificDatasetAsync(Study study, Dataset dataset, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"DeleteResourcesForStudySpecificDatasetAsync - Dataset Id: {dataset.Id}. Resources will be marked ");

            try
            {
                var datasetResourceEntry = DatasetUtils.GetStudySpecificStorageAccountResourceEntry(dataset);

                if (datasetResourceEntry != null)
                {
                    await SoftDeleteUtil.MarkAsDeleted(datasetResourceEntry, _userService);

                    var deleteOperation = await _cloudResourceOperationCreateService.CreateDeleteOperationAsync(datasetResourceEntry.Id, $"Delete dataset related resurces for dataset {dataset.Id}");
                    await ProvisioningQueueUtil.CreateItemAndEnqueue(_provisioningQueueService, deleteOperation);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete resources for study specific dataset", ex);
            }
        }       
    }
}
