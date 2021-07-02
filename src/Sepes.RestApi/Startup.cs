using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Sepes.Azure.Service;
using Sepes.Azure.Service.Interface;
using Sepes.Common.Constants;
using Sepes.Common.Interface;
using Sepes.Common.Util;
using Sepes.Infrastructure.Automapper;
using Sepes.Infrastructure.Handlers;
using Sepes.Infrastructure.Handlers.Interface;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Provisioning.Service;
using Sepes.Provisioning.Service.Interface;
using Sepes.RestApi.Middelware;
using Sepes.RestApi.Services;
using Sepes.RestApi.Services.GraphApi;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;


namespace Sepes.RestApi
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        readonly ILogger _logger;
        readonly IConfiguration _configuration;

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            Log("Sepes Startup Constructor");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log("ConfigureServices starting");

            AddApplicationInsights(services);

            services.AddControllers();

            var corsDomainsFromConfig = ConfigUtil.GetCommaSeparatedConfigValueAndThrowIfEmpty(_configuration, ConfigConstants.ALLOW_CORS_DOMAINS);

            Log("Startup - ConfigureServices - Cors domains: *");

            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    var domainsAsArray = new string[corsDomainsFromConfig.Count];
                    corsDomainsFromConfig.CopyTo(domainsAsArray);

                    builder.WithOrigins(domainsAsArray);
                    builder.AllowAnyHeader().AllowAnyMethod();
                });
            });

            var isIntegrationTest = ConfigUtil.GetBoolConfig(_configuration, ConfigConstants.IS_INTEGRATION_TEST);

            Log($"Is Integration test: {isIntegrationTest}");

            if (!isIntegrationTest)
            {
                var enableSensitiveDataLoggingFromConfig = ConfigUtil.GetBoolConfig(_configuration, ConfigConstants.SENSITIVE_DATA_LOGGING);

                var readWriteDbConnectionString = _configuration[ConfigConstants.DB_READ_WRITE_CONNECTION_STRING];
                DoMigration(enableSensitiveDataLoggingFromConfig);

                if (string.IsNullOrWhiteSpace(readWriteDbConnectionString))
                {
                    throw new Exception("Could not obtain database READWRITE connection string. Unable to add DB Context");
                }

                services.AddDbContext<SepesDbContext>(
                  options => options.UseSqlServer(
                      readWriteDbConnectionString,
                      assembly => assembly.MigrationsAssembly(typeof(SepesDbContext).Assembly.FullName))
                  .EnableSensitiveDataLogging(enableSensitiveDataLoggingFromConfig)
                  );
            }

            var authenticationAdder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(a => { }, b =>
                {
                    _configuration.Bind("AzureAd", b);

                    var defaultBackChannel = new HttpClient();
                    defaultBackChannel.DefaultRequestHeaders.Add("Origin", "sepes");
                    b.Backchannel = defaultBackChannel;

                }).EnableTokenAcquisitionToCallDownstreamApi(e =>
                    {

                    }
                    )              
                .AddInMemoryTokenCaches();

            if (!isIntegrationTest)
            {
                authenticationAdder
                .AddDownstreamWebApi("GraphApi", _configuration.GetSection("GraphApi"))
                .AddDownstreamWebApi("WbsSearch",(a) => { a.BaseUrl = _configuration[ConfigConstants.WBS_SEARCH_API_URL]; a.Scopes = _configuration[ConfigConstants.WBS_SEARCH_API_SCOPE]; });
            }

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(AutoMappingConfigs));

            RegisterServices(services, isIntegrationTest);

            SwaggerSetup.ConfigureServices(_configuration, services);

            Log("Configuring services done");
        }

        void AddApplicationInsights(IServiceCollection services)
        {
            Trace.WriteLine("Configuring Application Insights");

            var aiOptions = new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
                InstrumentationKey = _configuration[ConfigConstants.APPI_KEY],
                EnableDebugLogger = true
            };

            services.AddApplicationInsightsTelemetry(aiOptions);
        }

        void RegisterServices(IServiceCollection services, bool isIntegrationTest)
        {
            Log("Register services");

            if (isIntegrationTest)
            {
                Log("Is Integration test, adding HTTP client");
                services.AddHttpClient();
            }
            else
            {
                Log("Is NOT Integration test, adding HTTP client for services");
                //Services that use HttpClient, this registers both HttpClient and the service it self in same line
                services.AddHttpClient<IAzureCostManagementService, AzureCostManagementService>();
                services.AddHttpClient<IAzureDiskPriceService, AzureDiskPriceService>();
                services.AddHttpClient<IAzureRoleAssignmentService, AzureRoleAssignmentService>();
                services.AddHttpClient<IAzureVirtualMachineOperatingSystemService, AzureVirtualMachineOperatingSystemService>();
                services.AddHttpClient<IWbsApiService, WbsApiService>();

                //Azure Services
                services.AddTransient<IAzureUserService, AzureUserService>();
                services.AddTransient<IUserFromGroupLookupService, UserFromGroupLookupService>();
                services.AddTransient<ICombinedUserLookupService, CombinedUserLookupService>();               
                services.AddTransient<IAzureResourceGroupService, AzureResourceGroupService>();
                services.AddTransient<IAzureNetworkSecurityGroupService, AzureNetworkSecurityGroupService>();
                services.AddTransient<IAzureBastionService, AzureBastionService>();
                services.AddTransient<IAzureVirtualNetworkService, AzureVirtualNetworkService>();
                services.AddTransient<IAzureVirtualMachineService, AzureVirtualMachineService>();
                services.AddTransient<IAzureVirtualMachineExtendedInfoService, AzureVirtualMachineExtendedInfoService>();
                services.AddTransient<IAzureQueueService, AzureQueueService>();
                services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();
                services.AddTransient<IAzureBlobStorageUriBuilderService, AzureBlobStorageUriBuilderService>();
                services.AddTransient<IAzureStorageAccountService, AzureStorageAccountService>();
                services.AddTransient<IAzureStorageAccountAccessKeyService, AzureStorageAccountAccessKeyService>();
                services.AddTransient<IAzureStorageAccountNetworkRuleService, AzureStorageAccountNetworkRuleService>();
                services.AddTransient<IAzureNetworkSecurityGroupRuleService, AzureNetworkSecurityGroupRuleService>();
                services.AddTransient<IAzureResourceSkuService, AzureResourceSkuService>();               
                services.AddTransient<IAzureKeyVaultSecretService, AzureKeyVaultSecretService>();
            }

            //Plumbing          
            services.AddTransient<IDatabaseConnectionStringProvider, DatabaseConnectionStringProvider>();
            services.AddTransient<IRequestIdService, RequestIdService>();          
            services.AddTransient<IGraphServiceProvider, GraphServiceProvider>();
            services.AddSingleton<IPublicIpFromThirdPartyService, PublicIpFromThirdPartyService>();
            services.AddSingleton<IPublicIpService, PublicIpService>();
            services.AddTransient<IHealthService, HealthService>();
            services.AddTransient<IDapperQueryService, DapperQueryService>(); 
            
            //Authentication and Authorization
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<IContextUserService, ContextUserService>();            
            services.AddTransient<IUserModelService, UserModelDapperService>();
            services.AddTransient<IStudyPermissionService, StudyPermissionService>();
            services.AddTransient<IOperationPermissionService, OperationPermissionService>();
            services.AddTransient<IUserPermissionService, UserPermissionService>();

            //Data model services v2

            services.AddTransient<IStudyEfModelOperationsService, StudyEfModelOperationsService>();
            services.AddTransient<IStudyEfModelService, StudyEfModelService>();
            services.AddTransient<IStudyListModelService, StudyListModelService>();
            services.AddTransient<IStudyDetailsModelService, StudyDetailsModelService>();
            services.AddTransient<IStudyResultsAndLearningsModelService, StudyResultsAndLearningsModelService>();
            services.AddTransient<ISandboxModelService, SandboxModelService>();
            services.AddTransient<IPreApprovedDatasetModelService, PreApprovedDatasetModelService>();
            services.AddTransient<IStudySpecificDatasetModelService, StudySpecificDatasetModelService>();
            services.AddTransient<ISandboxDatasetModelService, SandboxDatasetModelService>();
            services.AddTransient<IResourceOperationModelService, ResourceOperationModelService>();
            services.AddTransient<IWbsCodeCacheModelService, WbsCodeCacheModelService>();

            //Use case handlers
            services.AddTransient<IStudyUpdateHandler, StudyUpdateHandler>();
            services.AddTransient<IStudyCreateLogoHandler, StudyCreateLogoHandler>();
            services.AddTransient<IStudyWbsUpdateHandler, StudyWbsUpdateHandler>();
            services.AddTransient<IStudyResultsAndLearningsUpdateHandler, StudyResultsAndLearningsUpdateHandler>();

            //Domain Model Services
            services.AddTransient<IStudyListService, StudyListService>();
            services.AddTransient<IStudyDetailsService, StudyDetailsService>();        
            services.AddTransient<IStudyCreateService, StudyCreateService>();
       
            services.AddTransient<IStudyDeleteService, StudyDeleteService>();
            services.AddTransient<IDatasetService, DatasetService>();
            services.AddTransient<IDatasetFirewallService, DatasetFirewallService>();
            services.AddTransient<IDatasetWaitForFirewallOperationService, DatasetWaitForFirewallOperationService>();            
            services.AddTransient<ISandboxService, SandboxService>();
            services.AddTransient<ISandboxPhaseService, SandboxPhaseService>();
            services.AddTransient<ISandboxResourceReadService, SandboxResourceReadService>();
            services.AddTransient<IStudyDatasetService, StudyDatasetService>();
            services.AddTransient<IStudyParticipantSearchService, StudyParticipantSearchService>();
            services.AddTransient<IStudyParticipantCreateService, StudyParticipantCreateService>();
            services.AddTransient<IStudyParticipantRemoveService, StudyParticipantRemoveService>();
            services.AddTransient<ICloudResourceReadService, CloudResourceReadService>();
            services.AddTransient<ICloudResourceCreateService, CloudResourceCreateService>();
            services.AddTransient<ICloudResourceUpdateService, CloudResourceUpdateService>();
            services.AddTransient<ICloudResourceDeleteService, CloudResourceDeleteService>();
            services.AddTransient<ICloudResourceOperationCreateService, CloudResourceOperationCreateService>();
            services.AddTransient<ICloudResourceOperationReadService, CloudResourceOperationReadService>();
            services.AddTransient<ICloudResourceOperationUpdateService, CloudResourceOperationUpdateService>();
            services.AddTransient<IWbsValidationService, WbsValidationService>();
            services.AddTransient<IStudyWbsValidationService, StudyWbsValidationService>();            

            services.AddTransient<IRegionService, RegionService>();
            services.AddScoped<IVariableService, VariableService>();
            services.AddTransient<IStudyParticipantRolesService, StudyParticipantRolesService>();        
            

            //Provisioning service
            services.AddTransient<IProvisioningLogService, ProvisioningLogService>();
            services.AddTransient<ICloudResourceMonitoringService, CloudResourceMonitoringService>();
            services.AddTransient<IResourceProvisioningService, ResourceProvisioningService>();
            services.AddTransient<IRoleProvisioningService, RoleProvisioningService>();
            services.AddTransient<IParticipantRoleTranslatorService, ParticipantRoleTranslatorService>();
            services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
            services.AddTransient<ICorsRuleProvisioningService, CorsRuleProvisioningService>();
            services.AddTransient<ICreateAndUpdateService, CreateAndUpdateService>();
            services.AddTransient<IDeleteOperationService, DeleteOperationService>();
            services.AddTransient<IFirewallService, FirewallService>();
            services.AddTransient<IOperationCheckService, OperationCheckService>();
            services.AddTransient<IOperationCompletedService, OperationCompletedService>();
            services.AddTransient<ITagProvisioningService, TagProvisioningService>();
           
            //Ext System Facade Services           
            services.AddTransient<IDatasetFileService, DatasetFileService>();
            services.AddTransient<IStudyLogoCreateService, StudyLogoCreateService>();
            services.AddTransient<IStudyLogoReadService, StudyLogoReadService>();
            services.AddTransient<IStudyLogoDeleteService, StudyLogoDeleteService>();
            services.AddTransient<IStudySpecificDatasetService, StudySpecificDatasetService>();          
         
            services.AddTransient<ISandboxResourceCreateService, SandboxResourceCreateService>();
            services.AddTransient<ISandboxResourceRetryService, SandboxResourceRetryService>();
            services.AddTransient<ISandboxResourceDeleteService, SandboxResourceDeleteService>();
            services.AddTransient<ICloudResourceMonitoringService, CloudResourceMonitoringService>();
            services.AddTransient<IVirtualMachineCreateService, VirtualMachineCreateService>();
            services.AddTransient<IVirtualMachineReadService, VirtualMachineReadService>();
            services.AddTransient<IVirtualMachineDeleteService, VirtualMachineDeleteService>();
            services.AddTransient<IVirtualMachineSizeService, VirtualMachineSizeService>();
            services.AddTransient<IVirtualMachineDiskSizeService, VirtualMachineDiskSizeService>();
            services.AddTransient<IVirtualMachineOperatingSystemService, VirtualMachineOperatingSystemService>();
            services.AddTransient<IVirtualMachineRuleService, VirtualMachineRuleService>();
            services.AddTransient<IVirtualMachineValidationService, VirtualMachineValidationService>();
            services.AddTransient<IDatasetCloudResourceService, DatasetCloudResourceService>();

            //Import Services
            services.AddTransient<IVirtualMachineDiskSizeImportService, VirtualMachineDiskSizeImportService>();
            services.AddTransient<IVirtualMachineSizeImportService, VirtualMachineSizeImportService>();
            Log("Register services done");
        }

        void DoMigration(bool enableSensitiveDataLogging)
        {
            var disableMigrations = _configuration[ConfigConstants.DISABLE_MIGRATIONS];

            if (!String.IsNullOrWhiteSpace(disableMigrations) && disableMigrations.ToLower() == "true")
            {
                Log("Migrations are disabled and will be skipped!");
                return;
            }
            else
            {
                Log("Performing database migrations");
            }

            string sqlConnectionStringOwner = GetConnectionString(ConfigConstants.DB_OWNER_CONNECTION_STRING, enableSensitiveDataLogging); // _configuration[ConfigConstants.DB_OWNER_CONNECTION_STRING];

            var createDbOptions = new DbContextOptionsBuilder<SepesDbContext>();
            createDbOptions.UseSqlServer(sqlConnectionStringOwner);
            createDbOptions.EnableSensitiveDataLogging(enableSensitiveDataLogging);

            using (var ctx = new SepesDbContext(createDbOptions.Options))
            {
                ctx.Database.SetCommandTimeout(300);
                ctx.Database.Migrate();
            }

            Log("Do migration done");
        }

        string GetConnectionString(string name, bool enableSensitiveDataLogging)
        {
            var connectionStringFromConfig = _configuration[name];

            if (string.IsNullOrWhiteSpace(connectionStringFromConfig))
            {
                throw new Exception($"Could not obtain database connection string with name: {name}.");
            }

            //Clean and print connection string (password will be removed)
            if (enableSensitiveDataLogging)
            {
                var cleanConnectionString = ConfigUtil.RemovePasswordFromConnectionString(connectionStringFromConfig);
                Log($"Connection string named {name}: {cleanConnectionString}");
            }

            return connectionStringFromConfig;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Log("Configure");

            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);

            //To get actual Client IP even though behind load balancer
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var httpOnlyRaw = _configuration["HttpOnly"];

            // UseHttpsRedirection doesn't work well with docker.        
            if (!String.IsNullOrWhiteSpace(httpOnlyRaw) && httpOnlyRaw.ToLower() == "true")
            {
                Log("Using HTTP only");
            }
            else
            {
                Log("Also using HTTPS. Activating https redirection");
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            SwaggerSetup.Configure(_configuration, app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Log("Configure done");
        }
        void Log(string message)
        {
            Trace.WriteLine(message);
            _logger.LogInformation(message);
        }
    }
}
