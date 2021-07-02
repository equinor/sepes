using Sepes.Common.Dto.VirtualMachine;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using Xunit;

namespace Sepes.Tests.Services.DomainServices.VirtualMachine
{
    public class VirtualMachineServiceTests : TestBaseWithInMemoryDb
    {
        public VirtualMachineServiceTests()
           : base()
        {

        }


        [Fact]
        public async void CreateVmWithInvalidPassword()
        {
            var invalidPassword = "123";
            var service = VirtualMachineMockServiceFactory.GetVirtualMachineCreateService(_serviceProvider);
            var validationOfName = service.CreateAsync(1, new VirtualMachineCreateDto { Password= invalidPassword });

            await Assert.ThrowsAsync<System.Exception>(async () => await validationOfName);
        }

        [Fact]
        public async void CheckvalidPassword()
        {
            var validPassword = "!1Qwertyuiop";
            var service = VirtualMachineMockServiceFactory.GetVirtualMachineCreateService(_serviceProvider);
            service.ValidateVmPasswordOrThrow(validPassword);
        }
    }
}
