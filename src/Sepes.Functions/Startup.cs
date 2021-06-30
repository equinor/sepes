using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Sepes.Azure.Service;
using Sepes.Azure.Service.Interface;
using Sepes.Common.Constants;
using Sepes.Common.Interface;
using Sepes.Functions.Service;
using Sepes.Infrastructure.Automapper;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Provisioning.Service;
using Sepes.Provisioning.Service.Interface;
using System;
using System.Diagnostics;

[assembly: FunctionsStartup(typeof(Sepes.Functions.Startup))]


namespace Sepes.Functions
{
    public class Startup : FunctionsStartup
    {       
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Log("Function - Startup - Configure - Start");

            var appiKey = GetConfigValue(ConfigConstants.APPI_KEY, true);
            var aiOptions = new ApplicationInsightsServiceOptions
            {               
                EnableAdaptiveSampling = false,
                InstrumentationKey = appiKey,
                EnableDebugLogger = true
            };         

            builder.Services.AddApplicationInsightsTelemetry(aiOptions);

            Log("Function - Startup - Configure - Initializing EF Core");

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
                  })
              );

            // This is configuration from environment variables, settings.json etc.
            var configuration = builder.GetContext().Configuration;

            Log("Function - Startup - Configure - Auth");
            builder.Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = "Bearer";
                sharedOptions.DefaultChallengeScheme = "Bearer";
            })
                .AddMicrosoftIdentityWebApi(configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddInMemoryTokenCaches();

            builder.Services.AddHttpContextAccessor();

            Log("Function - Startup - Configure - Adding Services");

            //Plumbing
            builder.Services.AddAutoMapper(typeof(AutoMappingConfigs));           
            builder.Services.AddScoped<IUserService, FunctionUserService>();          
            builder.Services.AddTransient<IRequestIdService, RequestIdService>();
            builder.Services.AddSingleton<IPublicIpFromThirdPartyService, PublicIpFromThirdPartyService>();
            builder.Services.AddSingleton<IPublicIpService, PublicIpService>();
            builder.Services.AddTransient<IHealthService, HealthService>();

            //Domain Model Services
            builder.Services.AddTransient<IDatabaseConnectionStringProvider, DatabaseConnectionStringProvider>();
            builder.Services.AddTransient<IStudyParticipantRolesService, StudyParticipantRolesService>();
            builder.Services.AddTransient<IStudyEfModelService, StudyEfModelService>();
            builder.Services.AddTransient<IDatasetService, DatasetService>();
            builder.Services.AddTransient<ISandboxService, SandboxService>();         
            builder.Services.AddScoped<IVariableService, VariableService>();
            builder.Services.AddTransient<ICloudResourceReadService, CloudResourceReadService>();
            builder.Services.AddTransient<ICloudResourceCreateService, CloudResourceCreateService>();
            builder.Services.AddTransient<ICloudResourceUpdateService, CloudResourceUpdateService>();
            builder.Services.AddTransient<IResourceOperationModelService, ResourceOperationModelService>();

            builder.Services.AddTransient<ICloudResourceOperationCreateService, CloudResourceOperationCreateService>();
            builder.Services.AddTransient<ICloudResourceOperationReadService, CloudResourceOperationReadService>();
            builder.Services.AddTransient<ICloudResourceOperationUpdateService, CloudResourceOperationUpdateService>();

            builder.Services.AddTransient<ISandboxResourceCreateService, SandboxResourceCreateService>();
            builder.Services.AddTransient<ISandboxResourceRetryService, SandboxResourceRetryService>();
            builder.Services.AddTransient<ISandboxResourceDeleteService, SandboxResourceDeleteService>();
            builder.Services.AddTransient<IWbsCodeCacheModelService, WbsCodeCacheModelService>();


            //Provisioning service            
            builder.Services.AddTransient<IProvisioningLogService, ProvisioningLogService>();
            builder.Services.AddTransient<ICloudResourceMonitoringService, CloudResourceMonitoringService>();
            builder.Services.AddTransient<IResourceProvisioningService, ResourceProvisioningService>();
            builder.Services.AddTransient<IRoleProvisioningService, RoleProvisioningService>();
            builder.Services.AddTransient<IParticipantRoleTranslatorService, ParticipantRoleTranslatorService>();
            builder.Services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
            builder.Services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
            builder.Services.AddTransient<ICorsRuleProvisioningService, CorsRuleProvisioningService>();
            builder.Services.AddTransient<ICreateAndUpdateService, CreateAndUpdateService>();
            builder.Services.AddTransient<IDeleteOperationService, DeleteOperationService>();
            builder.Services.AddTransient<IFirewallService, FirewallService>();
            builder.Services.AddTransient<IOperationCheckService, OperationCheckService>();
            builder.Services.AddTransient<IOperationCompletedService, OperationCompletedService>();
            builder.Services.AddTransient<ITagProvisioningService, TagProvisioningService>();           


            //IMPORT SERVICE
            builder.Services.AddTransient<IVirtualMachineDiskSizeImportService, VirtualMachineDiskSizeImportService>();
            builder.Services.AddTransient<IVirtualMachineSizeImportService, VirtualMachineSizeImportService>();

            //Azure Services
            builder.Services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();
            builder.Services.AddTransient<IAzureResourceGroupService, AzureResourceGroupService>();
            builder.Services.AddTransient<IAzureNetworkSecurityGroupService, AzureNetworkSecurityGroupService>();
            builder.Services.AddTransient<IAzureNetworkSecurityGroupRuleService, AzureNetworkSecurityGroupRuleService>();
            builder.Services.AddTransient<IAzureBastionService, AzureBastionService>();
            builder.Services.AddTransient<IAzureVirtualNetworkService, AzureVirtualNetworkService>();
            builder.Services.AddTransient<IAzureVirtualMachineService, AzureVirtualMachineService>();
            builder.Services.AddTransient<IAzureQueueService, AzureQueueService>();
            builder.Services.AddTransient<IAzureStorageAccountService, AzureStorageAccountService>();
            builder.Services.AddTransient<IAzureStorageAccountAccessKeyService, AzureStorageAccountAccessKeyService>();
            builder.Services.AddTransient<IAzureStorageAccountCorsRuleService, AzureStorageAccountCorsRuleService>();
            builder.Services.AddTransient<IAzureStorageAccountNetworkRuleService, AzureStorageAccountNetworkRuleService>();
            builder.Services.AddTransient<IAzureCostManagementService, AzureCostManagementService>();
            builder.Services.AddTransient<IAzureResourceSkuService, AzureResourceSkuService>();
            builder.Services.AddTransient<IAzureDiskPriceService, AzureDiskPriceService>();
            builder.Services.AddTransient<IAzureRoleAssignmentService, AzureRoleAssignmentService>();
            builder.Services.AddTransient<IAzureKeyVaultSecretService, AzureKeyVaultSecretService>();

            Log("Function - Startup - Configure - End");
        }

        string GetConfigValue(string key, bool throwIfEmpty = false)
        {
            var value = System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);

            if (throwIfEmpty && String.IsNullOrWhiteSpace(value))
            {
                Log($"Function - Startup - GetConfigValue: Unable to get config: {key}");
                throw new NullReferenceException($"Configuration {key} is null or empty");
            }

            return value;
        }

        void Log(string message)
        {
            Trace.WriteLine(message);
            //_logger.LogWarning(message);
        }
    }
}
