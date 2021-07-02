using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using Xunit;

namespace Sepes.Tests.Infrastructure.Util
{
    public class AzureVmSizeUtilTest
    {
     
        [Fact]
        public void Vm_getSizeCategory()
        {
            var vmSize = new VmSize() { Key = "first", NumberOfCores = 4, MemoryGB = 3, OsDiskSizeInMB = 1000, MaxDataDiskCount = 4 };

            var result = VmSizeUtil.GetDisplayTextSizeForDropdown(vmSize);

            var expectedResult = "first (4 cores, 3 GB Memory, os disk: 1000, max data disks: 4)";

            Assert.Equal(expectedResult, result);

        }

        [Fact]
        public void Vm_getSizeCategory_shouldReturnEmptyString()
        {
            var result = VmSizeUtil.GetDisplayTextSizeForDropdown(null);

            var expectedResult = "unknown";

            Assert.Equal(expectedResult, result);
        }

      
    }
}
