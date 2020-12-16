﻿
using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sepes.CloudResourceWorker.Service;
using Sepes.Infrastructure.Interface;
using Sepes.Infrastructure.Model.Automapper;
using Sepes.Infrastructure.Model.Config;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.Azure;
using Sepes.Infrastructure.Service.Azure.Interface;
using Sepes.Infrastructure.Service.Interface;
using System;

[assembly: FunctionsStartup(typeof(Sepes.CloudResourceWorker.Startup))]


namespace Sepes.CloudResourceWorker
{
    public class Startup : FunctionsStartup
    {

        string GetConfigValue(string key, bool throwIfEmpty = false)
        {
            var value = System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);

            if (throwIfEmpty && String.IsNullOrWhiteSpace(value))
            {
                throw new NullReferenceException($"Configuration {key} is null or empty");
            }

            return value;
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var appiKey = GetConfigValue(ConfigConstants.APPI_KEY, true);
            Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions aiOptions
             = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
            // Disables adaptive sampling.
            aiOptions.EnableAdaptiveSampling = false;
            aiOptions.InstrumentationKey = appiKey;
            aiOptions.EnableDebugLogger = true;

            builder.Services.AddApplicationInsightsTelemetry(aiOptions);

            var readWriteDbConnectionString = GetConfigValue(ConfigConstants.DB_READ_WRITE_CONNECTION_STRING, true);

            builder.Services.AddDbContext<SepesDbContext>(
              options => options.UseSqlServer(
                  readWriteDbConnectionString,
                  sqlServerOptionsAction: sqlOptions =>
                  {
                      sqlOptions.EnableRetryOnFailure(
                      maxRetryCount: 3,
                      maxRetryDelay: TimeSpan.FromSeconds(30),
                      errorNumbersToAdd: null);
                  }
                  )

              );

            builder.Services.AddHttpContextAccessor();

            //Plumbing
            builder.Services.AddAutoMapper(typeof(AutoMappingConfigs));           
            builder.Services.AddScoped<IUserService, FunctionUserService>();
            builder.Services.AddTransient<IRequestIdService, RequestIdService>();

            //Domain Model Services
            builder.Services.AddTransient<ILookupService, LookupService>();
            builder.Services.AddTransient<IDatasetService, DatasetService>();
            builder.Services.AddTransient<IStudyParticipantService, StudyParticipantService>();
            builder.Services.AddTransient<ISandboxService, SandboxService>();
            builder.Services.AddTransient<IStudyService, StudyService>();
            builder.Services.AddScoped<IVariableService, VariableService>();
            builder.Services.AddTransient<ISandboxResourceService, SandboxResourceService>();

            //Ext System Facade Services
            builder.Services.AddTransient<IResourceProvisioningService, ResourceProvisioningService>();
            builder.Services.AddTransient<ISandboxResourceMonitoringService, SandboxResourceMonitoringService>();
            builder.Services.AddTransient<ISandboxResourceOperationService, SandboxResourceOperationService>();
            builder.Services.AddTransient<ISandboxCloudResourceService, SandboxCloudResourceService>();
            builder.Services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
            builder.Services.AddTransient<IVirtualMachineSizeService, VirtualMachineSizeService>();
       

            //Azure Services
            builder.Services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();
            builder.Services.AddTransient<IAzureResourceGroupService, AzureResourceGroupService>();
            builder.Services.AddTransient<IAzureNetworkSecurityGroupService, AzureNetworkSecurityGroupService>();
            builder.Services.AddTransient<IAzureNetworkSecurityGroupRuleService, AzureNetworkSecurityGroupRuleService>();
            builder.Services.AddTransient<IAzureBastionService, AzureBastionService>();
            builder.Services.AddTransient<IAzureVNetService, AzureVNetService>();
            builder.Services.AddTransient<IAzureVmService, AzureVmService>();
            builder.Services.AddTransient<IAzureQueueService, AzureQueueService>();
            builder.Services.AddTransient<IAzureStorageAccountService, AzureStorageAccountService>();
            builder.Services.AddTransient<IAzureCostManagementService, AzureCostManagementService>();
            builder.Services.AddTransient<IAzureResourceSkuService, AzureResourceSkuService>();

        }
    }
}
