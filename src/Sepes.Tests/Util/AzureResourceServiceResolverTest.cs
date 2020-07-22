﻿using Microsoft.Extensions.DependencyInjection;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Util;
using Sepes.Tests.Setup;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sepes.Tests.Util
{
    public class AzureResourceServiceResolverRest
    {
        public ServiceCollection Services { get; private set; }
        public ServiceProvider ServiceProvider { get; protected set; }

        public AzureResourceServiceResolverRest()
        {
            Services = BasicServiceCollectionFactory.GetServiceCollectionWithInMemory();

            ServiceProvider = Services.BuildServiceProvider();
        }

        [Fact]
        public async void ResolvingServiceForResourceWithProvisioningStateShouldBeOkay()
        {
            //Trying resource group
            var shouldBeNull = AzureResourceServiceResolver.GetServiceWithProvisioningState(ServiceProvider, "SomeResourceThatDoesNotExist");

            Assert.Null(shouldBeNull);    


            //Trying resource group
            var resourceGroupService = AzureResourceServiceResolver.GetServiceWithProvisioningState(ServiceProvider, "ResourceGroup");

            Assert.NotNull(resourceGroupService);
            Assert.IsType<AzureResourceGroupService>(resourceGroupService);

            //Trying VNet
            var vNetService = AzureResourceServiceResolver.GetServiceWithProvisioningState(ServiceProvider, "Network");

            Assert.NotNull(vNetService);
            Assert.IsType<AzureVNetService>(vNetService);


            //Trying Bastion
            var bastionService = AzureResourceServiceResolver.GetServiceWithProvisioningState(ServiceProvider, "Bastion");

            Assert.NotNull(bastionService);
            Assert.IsType<AzureBastionService>(bastionService);
        }
    }
}