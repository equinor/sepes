using System;
using System.Collections.Generic;
using Sepes.Azure.Util;
using Sepes.Common.Dto.VirtualMachine;
using Xunit;

namespace Sepes.Tests.Util
{
    public class AzureVmUtilTest
    {
        [Fact]
        public void VmRule_IsSameRule_withSameRule_shouldBeTrue()
        {
            var rule1 = new VmRuleDto(){ Direction = 0, Protocol = "tcp", Ip= "192.168.1.1", Port= 443 };
            var rule2 = new VmRuleDto(){ Direction = 0, Protocol = "tcp", Ip = "192.168.1.1", Port = 443 };
            var result = AzureVmUtil.IsSameRule(rule1, rule2);

            Assert.True(result);

        }

        [Fact]
        public void VmRule_IsSameRule_witthDifferentRules_shouldBeFalse()
        {
            var rule1 = new VmRuleDto() { Direction = 0, Protocol = "tcp", Ip = "192.168.1.1", Port = 443 };
            var rule2 = new VmRuleDto() { Direction = 0, Protocol = "tcp", Ip = "192.168.1.1", Port = 80 };
            var result = AzureVmUtil.IsSameRule(rule1, rule2);

            Assert.False(result);

        }

        [Theory]
        [InlineData("asdasdstandard_e", "memory")]
        [InlineData("standard_nv", "gpu")]
        [InlineData("standard_f", "compute")]
        [InlineData("asdasdasdasd", "unknowncategory")]
        [InlineData("", "unknowncategory")]
        [InlineData(null, "unknowncategory")]
        public void Vm_getSizeCategory_ShouldReturn_string(string vmName, string expectedResult)
        {

            var result = AzureVmUtil.GetSizeCategory(vmName);

            Assert.Equal(expectedResult, result);

        }

        [Fact]
        public void Vm_getDiskSize_shouldReturnGBDiskSize()
        {
            var result = AzureVmUtil.GetDiskSizeDisplayTextForDropdown(5);

            var expectedResult = "5 GB";

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Vm_getOsCategory_shouldReturnExpectedResult()
        {
            List<VmOsDto> osList = new List<VmOsDto>();
            osList.Add(new VmOsDto() { Key = "windows1", Category = "test1", DisplayValue = "disp1" });

            var result = AzureVmUtil.GetOsCategory(osList, "windows1");

            var expectedResult = "test1";

            Assert.Equal(expectedResult, result );
        }

        [Fact]
        public void Vm_getOsCategory_shouldThrowWithcorrectMessage()
        {
            List<VmOsDto> osList = new List<VmOsDto>();
            osList.Add(new VmOsDto() { Key = "windows1", Category = "test1", DisplayValue = "disp1" });

            var ex = Assert.Throws<Exception>(() => AzureVmUtil.GetOsCategory(osList, "asdasas"));

            Assert.Equal("Unable to find Operating System record in list of available", ex.Message);
        }

        [Fact]
        public void Vm_getOsCategory_WithNullasOsList_shouldThrowWithcorrectMessage()
        {
            var ex = Assert.Throws<ArgumentException>(() => AzureVmUtil.GetOsCategory(null, "aaa"));

            Assert.Equal("List of OS is null", ex.Message);
        }

        [Fact]
        public void Vm_getPowerState_WithNull_shouldReturnString()
        {
            var result = AzureVmUtil.GetPowerState(null);

            Assert.Equal("not found", result);
        }
    }
}
