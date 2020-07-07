﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Sepes.Infrastructure.Model.Automapper;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using System.Collections.Generic;

namespace Sepes.Tests.Setup
{
    public static class BasicServiceCollectionFactory
    {
        public static ServiceCollection GetServiceCollectionWithInMemory()
        {
            var services = new ServiceCollection();
            services.AddDbContext<SepesDbContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"),
             ServiceLifetime.Scoped,
             ServiceLifetime.Scoped);
            //IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables().Build();
            //config.GetSection("ConnectionStrings").Bind(new ConnectionStrings());
            services.AddTransient<IConfiguration, TestConfig>();
            services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();

            services.AddAutoMapper(typeof(AutoMappingConfigs));

            return services;
        }
    }

    public class ConnectionStrings
    {
        public string AzureStorageConnectionString { get; set; }
    }

    public class TestConfig : IConfiguration
    {
        readonly Dictionary<string, string> configKeys = new Dictionary<string, string>();

        public TestConfig()
        {
            this["AzureStorageConnectionString"] = "UseDevelopmentStorage=true";
        }
        public string this[string key] { get { return configKeys[key]; }  set { configKeys[key] = value; }  } 

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new System.NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new System.NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new System.NotImplementedException();
        }
    }

}