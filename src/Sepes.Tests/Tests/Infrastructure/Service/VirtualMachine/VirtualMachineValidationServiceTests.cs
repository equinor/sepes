using Sepes.Common.Constants;
using Sepes.Common.Dto.VirtualMachine;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services.DomainServices.VirtualMachine
{
    public class VirtualMachineValidationServiceTests: TestBaseWithInMemoryDb
    {
        public VirtualMachineValidationServiceTests()
           : base()
        {

        }


        [Theory]
        [InlineData("admin", AzureVmConstants.WINDOWS, false)]
        [InlineData("admin123", AzureVmConstants.WINDOWS, true)]
        [InlineData("admin123.", AzureVmConstants.WINDOWS, false)]
        [InlineData("user5", AzureVmConstants.WINDOWS, true)]
        [InlineData("user5", AzureVmConstants.LINUX, false)]
        public void GetVirtualMachineUserNameValdiation(string name, string operatingSystem, Boolean expectedResult)
        {
            var virtualMachineValidationService = VirtualMachineMockServiceFactory.GetVirtualMachineValidationService();
            var validationOfName = virtualMachineValidationService.CheckIfUsernameIsValidOrThrow(new VmUsernameDto { OperativeSystemType = operatingSystem, Username=name});

            Assert.Equal(validationOfName.isValid, expectedResult);
        }

        [Theory]
        [InlineData("study1", "sandbox1", "james", "vm-study1-sandbox1-james")]
        public void GetVirtualMachineNameValdiation(string studyName, string sandboxName, string prefix, string expectedResult)
        {
            var virtualMachineValidationService = VirtualMachineMockServiceFactory.GetVirtualMachineValidationService();
            var calculatedName = virtualMachineValidationService.CalculateName(studyName, sandboxName, prefix);

            Assert.Equal(calculatedName, expectedResult);
        }
    }
}
