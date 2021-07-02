using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Constants.CloudResource;
using Sepes.Common.Dto.VirtualMachine;
using Sepes.Common.Util;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using Sepes.RestApi.IntegrationTests.TestHelpers;
using Sepes.Tests.Common.Constants;
using System.Threading.Tasks;

namespace Sepes.RestApi.IntegrationTests.Setup.Seeding
{
    public static class VirtualMachineSeed
    {
        public static async Task<CloudResource> CreateSimple(
          Sandbox sandbox,
          string vmNameSuffix = VirtualMachineTestConstants.NAME)
        {
            var sandboxResourceGroup = CloudResourceUtil.GetSandboxResourceGroupEntry(sandbox.Resources);
            var vmResource = CreateVmResource(sandbox, sandboxResourceGroup, sandbox.Study.Name, vmNameSuffix);
            return await SliceFixture.InsertAsync(vmResource);
        }

        public static async Task<CloudResource> Create(
            Sandbox sandbox,
            string vmNameSuffix = VirtualMachineTestConstants.NAME,
            string size = VirtualMachineTestConstants.SIZE,
            string osCategory = VirtualMachineTestConstants.OS_CATEGORY_WINDOWS,
            string os = VirtualMachineTestConstants.OS_WINDOWS,
            bool deleted = false,
            bool deleteSucceeded = false)
        {
            var sandboxResourceGroup = CloudResourceUtil.GetSandboxResourceGroupEntry(sandbox.Resources);

            var vmSettings = CreateVmSettingsString(size, osCategory, os);

            var vmResource = CreateVmResource(sandbox, sandboxResourceGroup, sandbox.Study.Name, vmNameSuffix, vmSettings, deleted: deleted, deleteSucceeded: deleteSucceeded);

            return await SliceFixture.InsertAsync(vmResource);
        }

        public static async Task<CloudResource> CreateFailed(
          Sandbox sandbox,
          string vmNameSuffix = VirtualMachineTestConstants.NAME,
          string size = VirtualMachineTestConstants.SIZE,
          string osCategory = VirtualMachineTestConstants.OS_CATEGORY_WINDOWS,
          string os = VirtualMachineTestConstants.OS_WINDOWS,
          string statusOfFailedResource = CloudResourceOperationState.FAILED,
          int tryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT,
          int maxTryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT,
            bool deleted = false,
            bool deleteSucceeded = false)
        {
            var sandboxResourceGroup = CloudResourceUtil.GetSandboxResourceGroupEntry(sandbox.Resources);

            var vmSettings = CreateVmSettingsString(size, osCategory, os);

            var vmResource = CreateFailedVmResource(sandbox, sandboxResourceGroup, sandbox.Study.Name, vmNameSuffix, vmSettings, statusOfFailedResource, tryCount, maxTryCount, deleted: deleted, deleteSucceeded: deleteSucceeded);

            return await SliceFixture.InsertAsync(vmResource);
        }

        static CloudResource CreateVmResource(Sandbox sandbox,
            CloudResource sandboxResourceGroup,
            string studyName,
            string nameSuffix,
            string configString = null,
            bool deleted = false,
            bool deleteSucceeded = false)
        {
            var vmResourceName = AzureResourceNameUtil.VirtualMachine(studyName, sandbox.Name, nameSuffix);

            var vmResource = CloudResourceFactory.Create(sandboxResourceGroup.Region, AzureResourceType.VirtualMachine, sandboxResourceGroup.ResourceName, vmResourceName, parentResource: sandboxResourceGroup, deleted: deleted, deleteSucceeded: deleteSucceeded);
            vmResource.SandboxId = sandbox.Id;
            vmResource.ConfigString = configString;
            return vmResource;
        }

        static CloudResource CreateFailedVmResource(Sandbox sandbox,
            CloudResource sandboxResourceGroup,
            string studyName,
            string nameSuffix,
            string configString = null,
            string statusOfFailedResource = CloudResourceOperationState.FAILED,
            int tryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT,
            int maxTryCount = CloudResourceConstants.RESOURCE_MAX_TRY_COUNT,
            bool deleted = false,
            bool deleteSucceeded = false)
        {
            var vmResourceName = AzureResourceNameUtil.VirtualMachine(studyName, sandbox.Name, nameSuffix);

            var vmResource = CloudResourceFactory.CreateFailing(sandboxResourceGroup.Region, AzureResourceType.VirtualMachine, sandboxResourceGroup.ResourceName, vmResourceName, parentResource: sandboxResourceGroup, statusOfFailedResource: statusOfFailedResource, tryCount: tryCount, maxTryCount: maxTryCount, deleted: deleted, deleteSucceeded: deleteSucceeded);
            vmResource.SandboxId = sandbox.Id;
            vmResource.ConfigString = configString;
            return vmResource;
        }

        static string CreateVmSettingsString(string size = VirtualMachineTestConstants.SIZE, string osCategory = "windows", string os = "win2019datacenter")
        {
            var vmSettings = new VmSettingsDto()
            {
                DiagnosticStorageAccountName = "diagstorageaccountname",
                NetworkName = "networkName",
                SubnetName = "subnetname",
                Size = size,
                Rules = VmRuleUtils.CreateInitialVmRules(1),
                OperatingSystemCategory = osCategory,
                OperatingSystem = os,
                Username = VirtualMachineTestConstants.USERNAME,
                Password = "nameinkeyvault",
            };

            return CloudResourceConfigStringSerializer.Serialize(vmSettings);
        }
    }
}
