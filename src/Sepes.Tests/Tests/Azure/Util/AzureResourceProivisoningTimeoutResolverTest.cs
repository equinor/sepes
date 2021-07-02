using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Constants.CloudResource;
using Xunit;

namespace Sepes.Tests.Util
{
    public class AzureResourceProivisoningTimeoutResolverTest
    {
        [InlineData(AzureResourceType.StorageAccount, CloudResourceOperationType.CREATE, 180)]
        [InlineData(AzureResourceType.NetworkSecurityGroup, CloudResourceOperationType.CREATE, 180)]
        [InlineData(AzureResourceType.VirtualNetwork, CloudResourceOperationType.CREATE, 180)]
        [InlineData(AzureResourceType.ResourceGroup, CloudResourceOperationType.CREATE, 60)]
        [InlineData(AzureResourceType.ResourceGroup, CloudResourceOperationType.DELETE, 600)]
        [InlineData(AzureResourceType.Bastion, CloudResourceOperationType.CREATE, 900)]
        [InlineData(AzureResourceType.VirtualMachine, CloudResourceOperationType.CREATE, 600)]
        [InlineData("", "", 60)]
        [InlineData(null, null, 60)]
        [Theory]
        public void GetTimeoutForOperationInSeconds_ShouldContainStudyName(string resourceType, string operationType, int expectedResult)
        {
            var result = ResourceProivisoningTimeoutResolver.GetTimeoutForOperationInSeconds(resourceType, operationType);
            Assert.Equal(expectedResult, result);

        }
    }
}
