using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Constants.CloudResource;
using Sepes.Common.Model;
using Sepes.Infrastructure.Model;
using Sepes.RestApi.IntegrationTests.TestHelpers;
using Sepes.Tests.Common.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.RestApi.IntegrationTests.Setup.Seeding
{
    public static class SandboxSeed
    {
        public static async Task<Sandbox> Create(
                Study study,
                string sandboxName = SandboxTestConstants.NAME,
                string region = CommonTestConstants.REGION,
                SandboxPhase phase = SandboxPhase.Open,
             bool addDatasets = false)
        {
            var sandbox = SandboxBasic(study.Id, study.Name, sandboxName, region, SandboxResources(region, study.Name, sandboxName), phase);

            AddDatasets(addDatasets, study, sandbox);

            return await SliceFixture.InsertAsync(sandbox);
        }

        public static async Task<Sandbox> CreateFailing(
              Study study,
              string sandboxName = SandboxTestConstants.NAME,
              string region = CommonTestConstants.REGION,
              SandboxPhase phase = SandboxPhase.Open,
              int resourcesSucceeded = 0,
              string statusOfFailedResource = CloudResourceOperationState.FAILED,
              int tryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT,
              int maxTryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT,
                bool addDatasets = false)
        {
            var resources = SandboxResourcesFailing(region, study.Name, sandboxName, resourcesSucceeded, statusOfFailedResource, tryCount, maxTryCount);
            var sandbox = SandboxBasic(study.Id, study.Name, sandboxName, region, resources, phase);

            AddDatasets(addDatasets, study, sandbox);

            return await SliceFixture.InsertAsync(sandbox);
        }

        static Sandbox SandboxBasic(int studyId, string studyName, string sandboxName, string region, List<CloudResource> cloudResources, SandboxPhase phase = SandboxPhase.Open)
        {
            var sb = new Sandbox()
            {
                StudyId = studyId,
                Name = sandboxName,
                Region = region,
                PhaseHistory = new List<SandboxPhaseHistory>(),
                CreatedBy = "seed",
                Created = DateTime.UtcNow,
                UpdatedBy = "seed",
                Updated = DateTime.UtcNow,
                TechnicalContactEmail = "seedcreator@somesystem.com",
                TechnicalContactName = "Seed",
                Resources = cloudResources
            };

            int counter = 0;

            for (var curPhase = SandboxPhase.Open; curPhase <= phase; curPhase++)
            {
                sb.PhaseHistory.Add(new SandboxPhaseHistory() { Counter = counter, Phase = curPhase, CreatedBy = "seed" });
                counter++;
            }

            return sb;
        }

        static List<CloudResource> SandboxResources(string region, string studyName, string sandboxName)
        {
            var resourceGroupName = AzureResourceNameUtil.SandboxResourceGroup(studyName, sandboxName);
            var resourceGroup = CloudResourceFactory.CreateResourceGroup(region, resourceGroupName, purpose: CloudResourcePurpose.SandboxResourceGroup, sandboxControlled: true);

            var result = new List<CloudResource>() { resourceGroup };
            result.Add(CloudResourceFactory.Create(region, AzureResourceType.StorageAccount, resourceGroupName, AzureResourceNameUtil.DiagnosticsStorageAccount(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));
            result.Add(CloudResourceFactory.Create(region, AzureResourceType.NetworkSecurityGroup, resourceGroupName, AzureResourceNameUtil.NetworkSecGroup(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));
            result.Add(CloudResourceFactory.Create(region, AzureResourceType.VirtualNetwork, resourceGroupName, AzureResourceNameUtil.VNet(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));
            result.Add(CloudResourceFactory.Create(region, AzureResourceType.Bastion, resourceGroupName, AzureResourceNameUtil.Bastion(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));

            return result;
        }

        static List<CloudResource> SandboxResourcesFailing(string region, string studyName, string sandboxName,
                int resourcesSucceeded = 0,
              string statusOfFailedResource = CloudResourceOperationState.FAILED,
              int tryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT,
              int maxTryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT)
        {
            var resourceCreationIndex = 0;//number of resources created

            var resourceGroupName = AzureResourceNameUtil.SandboxResourceGroup(studyName, sandboxName);

            CloudResource resourceGroup;

            if(resourceCreationIndex++ < resourcesSucceeded)
            {
                resourceGroup = CloudResourceFactory.CreateResourceGroup(region, resourceGroupName, purpose: CloudResourcePurpose.SandboxResourceGroup, sandboxControlled: true);
            }
            else
            {
                resourceGroup = CloudResourceFactory.CreateResourceGroupFailing(region, resourceGroupName, purpose: CloudResourcePurpose.SandboxResourceGroup, sandboxControlled: true, statusOfFailedResource: statusOfFailedResource, tryCount: tryCount, maxTryCount: maxTryCount);
            }           

            var result = new List<CloudResource>() { resourceGroup };
            result.Add(CreateSucceedingOrFailing(resourcesSucceeded, resourceCreationIndex++, statusOfFailedResource, tryCount, maxTryCount, region, AzureResourceType.StorageAccount, resourceGroupName, AzureResourceNameUtil.DiagnosticsStorageAccount(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));
            result.Add(CreateSucceedingOrFailing(resourcesSucceeded, resourceCreationIndex++, statusOfFailedResource, tryCount, maxTryCount, region, AzureResourceType.NetworkSecurityGroup, resourceGroupName, AzureResourceNameUtil.NetworkSecGroup(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));
            result.Add(CreateSucceedingOrFailing(resourcesSucceeded, resourceCreationIndex++, statusOfFailedResource, tryCount, maxTryCount, region, AzureResourceType.VirtualNetwork, resourceGroupName, AzureResourceNameUtil.VNet(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));
            result.Add(CreateSucceedingOrFailing(resourcesSucceeded, resourceCreationIndex++, statusOfFailedResource, tryCount, maxTryCount, region, AzureResourceType.Bastion, resourceGroupName, AzureResourceNameUtil.Bastion(studyName, sandboxName), parentResource: resourceGroup, sandboxControlled: true));

            return result;
        }

        public static CloudResource CreateSucceedingOrFailing(int resourcesSucceeded, int resourceCreationIndex,string statusOfFailedResource, int tryCount,int maxTryCount,
              string region,
              string resourceType,
              string resourceGroup,
              string resourceName,
              string resourceId = null,
              string resourceKey = null,
              string purpose = null,
              bool sandboxControlled = false,
              string batchId = null,
              CloudResource parentResource = null)
        {
            return resourceCreationIndex == resourcesSucceeded?
                CloudResourceFactory.CreateFailing(region, resourceType, resourceGroup, resourceName, resourceId, resourceKey, purpose, sandboxControlled, batchId, parentResource, statusOfFailedResource, tryCount, maxTryCount) :
                CloudResourceFactory.Create(region, resourceType, resourceGroup, resourceName, resourceId, resourceKey, purpose, sandboxControlled, batchId, parentResource, resourceCreationIndex >= resourcesSucceeded ? false : true);
        }

        static void AddDatasets(bool addDatasets, Study study, Sandbox sandbox)
        {
            if (addDatasets)
            {
                if (study.StudyDatasets != null)
                {
                    sandbox.SandboxDatasets = new List<SandboxDataset>();

                    foreach (var curDs in study.StudyDatasets)
                    {
                        sandbox.SandboxDatasets.Add(new SandboxDataset() { DatasetId = curDs.DatasetId, Sandbox = sandbox, Added = DateTime.UtcNow, AddedBy = "seed" });
                    }
                }
            }

        }

    }
}
