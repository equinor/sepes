﻿using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.DependencyInjection;
using Sepes.Infrastructure.Service;
using Sepes.Tests.Setup;
using System;
using Xunit;

namespace Sepes.Tests.Services.Azure
{
    public class AzureSandboxServiceTest
    {
        public ServiceCollection Services { get; private set; }
        public ServiceProvider ServiceProvider { get; protected set; }

        public AzureSandboxServiceTest()
        {
            Services = BasicServiceCollectionFactory.GetServiceCollectionWithInMemory();
            Services.AddTransient<AzureService>();
            ServiceProvider = Services.BuildServiceProvider();
        }

        [Fact]    
        public async void CreatingSandboxShouldBeOk()
        {
            var sandboxService = ServiceProvider.GetService<AzureService>();

            var uniqueName = Guid.NewGuid().ToString().ToLower().Substring(0, 5);
            var studyName = $"utest-{uniqueName}";

            var sandbox = await sandboxService.CreateSandboxAsync(studyName, Region.EuropeWest);

            Assert.IsType<string>(sandbox);
        }

    }
}
