using Sepes.Common.Constants.CloudResource;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using Xunit;

namespace Sepes.Tests.Util
{
    public class AzureResourceStatusUtilTest
    {
        [Fact]
        public void DecideWhatOperationToBaseStatusOn_withEmptyParameter_shouldThrow()
        {
            var cloudResource = new CloudResource() { };

            var ex = Assert.Throws<ArgumentNullException>(() => ResourceStatusUtil.DecideWhatOperationToBaseStatusOn(cloudResource));
        }

        [Fact]
        public void DecideWhatOperationToBaseStatusOn_withEmptyList_shouldThrow()
        {
            var cloudResourceOperation = new List<CloudResourceOperation>() { };
            var cloudResource = new CloudResource() { Operations = cloudResourceOperation };
            var result = ResourceStatusUtil.DecideWhatOperationToBaseStatusOn(cloudResource);
            Assert.Null(result);
        }

        [Fact]
        public void DecideWhatOperationToBaseStatusOn_withValues_ShouldReturnExpected()
        {
            var cloudResourceOperationList = new List<CloudResourceOperation>() { };
            var cloudOperation1 = new CloudResourceOperation() { Status = CloudResourceOperationState.DONE_SUCCESSFUL };
            cloudResourceOperationList.Add(cloudOperation1);
            var cloudResource = new CloudResource() { Operations = cloudResourceOperationList };
            var result = ResourceStatusUtil.DecideWhatOperationToBaseStatusOn(cloudResource);
            Assert.Equal(cloudOperation1, result);
        }

        [Fact]
        public void DecideWhatOperationToBaseStatusOn_withValues_ShouldReturnExpected2()
        {
            var cloudResourceOperationList = new List<CloudResourceOperation>() { };
            var cloudOperation1 = new CloudResourceOperation() { Status = CloudResourceOperationState.FAILED };
            cloudResourceOperationList.Add(cloudOperation1);
            var cloudResource = new CloudResource() { Operations = cloudResourceOperationList };
            var result = ResourceStatusUtil.DecideWhatOperationToBaseStatusOn(cloudResource);
            Assert.Equal(cloudOperation1, result);
        }

        [Fact]
        public void DecideWhatOperationToBaseStatusOn_withValues_ShouldReturnExpected3()
        {
            var cloudResourceOperationList = new List<CloudResourceOperation>() { };
            var cloudOperation1 = new CloudResourceOperation() { Status = CloudResourceOperationState.ABORTED };
            var cloudOperation2 = new CloudResourceOperation() { Status = CloudResourceOperationState.DONE_SUCCESSFUL };
            cloudResourceOperationList.Add(cloudOperation1);
            cloudResourceOperationList.Add(cloudOperation2);
            var cloudResource = new CloudResource() { Operations = cloudResourceOperationList };
            var result = ResourceStatusUtil.DecideWhatOperationToBaseStatusOn(cloudResource);
            Assert.Equal(cloudOperation1, result);
        }

        [Fact]
        public void DecideWhatOperationToBaseStatusOn_withValues_ShouldReturnExpected4()
        {
            var cloudResourceOperationList = new List<CloudResourceOperation>() { };
            var cloudOperation1 = new CloudResourceOperation() { Status = CloudResourceOperationState.ABORTED };
            var cloudOperation2 = new CloudResourceOperation() { Status = CloudResourceOperationState.FAILED };
            cloudResourceOperationList.Add(cloudOperation1);
            cloudResourceOperationList.Add(cloudOperation2);
            var cloudResource = new CloudResource() { Operations = cloudResourceOperationList };
            var result = ResourceStatusUtil.DecideWhatOperationToBaseStatusOn(cloudResource);
            Assert.Equal(cloudOperation2, result);
        }

        [InlineData(CloudResourceOperationState.IN_PROGRESS, CloudResourceOperationType.CREATE, "Creating")]
        [InlineData(CloudResourceOperationState.IN_PROGRESS, CloudResourceOperationType.DELETE, "Deleting")]
        [InlineData(CloudResourceOperationState.IN_PROGRESS, CloudResourceOperationType.UPDATE, "Updating")]
        [InlineData(CloudResourceOperationState.NEW, CloudResourceOperationType.CREATE, "Creating (queued)")]
        [InlineData(CloudResourceOperationState.NEW, CloudResourceOperationType.DELETE, "Deleting (queued)")]
        [InlineData(CloudResourceOperationState.NEW, CloudResourceOperationType.UPDATE, "Updating (queued)")]
        [InlineData(CloudResourceOperationState.FAILED, CloudResourceOperationType.CREATE, "Create failed (0/0)")]
        [InlineData(CloudResourceOperationState.FAILED, CloudResourceOperationType.DELETE, "Delete failed (0/0)")]
        [InlineData(CloudResourceOperationState.FAILED, CloudResourceOperationType.UPDATE, "Update failed (0/0)")]
        [Theory]
        public void ResourceStatus_shouldReturnCorrectStatus(string status, string operationType, string expectedResult)
        {
            var cloudResourceOperationList = new List<CloudResourceOperation>() { };
            var cloudOperation1 = new CloudResourceOperation() { Status = status, OperationType = operationType };
            cloudResourceOperationList.Add(cloudOperation1);
            var cloudResource = new CloudResource() { Operations = cloudResourceOperationList };
            var result = ResourceStatusUtil.ResourceStatus(cloudResource);
            Assert.Equal(expectedResult, result);
        }

    }
}
