﻿using Sepes.Infrastructure.Dto.VirtualMachine;
using Sepes.Tests.Setup;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sepes.Tests.Services.DomainServices.VirtualMachine
{
    public class VirtualMachineServiceTests : ServiceTestBase
    {
        public VirtualMachineServiceTests()
           : base()
        {

        }


        [Fact]
        public async void CreateVmWithInvalidPassword()
        {
            var invalidPassword = "123";
            var virtualMachineLookupService = VirtualMachineMockFactory.GetVirtualMachineService(_serviceProvider);
            var validationOfName = virtualMachineLookupService.CreateAsync(1, new CreateVmUserInputDto { Password= invalidPassword });

            await Assert.ThrowsAsync<System.Exception>(async () => await validationOfName);
        }
    }
}