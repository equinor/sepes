﻿using AutoMapper;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
using Sepes.Infrastructure.Model.Automapper;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Provisioning.Service;
using Sepes.Provisioning.Service.Interface;
using Sepes.RestApi.Middelware;
using Sepes.RestApi.Services;
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

            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(a => { }, b =>
                {
                    _configuration.Bind("AzureAd", b);

                    var defaultBackChannel = new HttpClient();
                    defaultBackChannel.DefaultRequestHeaders.Add("Origin", "sepes");
                    b.Backchannel = defaultBackChannel;

                }).EnableTokenAcquisitionToCallDownstreamApi(e =>
                    {
                        //_configuration.Bind("GraphApi", e);
                    }
                    )
                 .AddDownstreamWebApi("GraphApi", _configuration.GetSection("GraphApi"))
                .AddDownstreamWebApi("WbsSearch", _configuration.GetSection("WbsSearch"))
                .AddInMemoryTokenCaches();

            services.AddHttpClient();

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(AutoMappingConfigs));

            RegisterServices(services);

            SetFileUploadLimits(services);

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

        void RegisterServices(IServiceCollection services)
        {
            //Plumbing
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserPermissionService, UserPermissionService>();
            services.AddScoped<IPrincipalService, PrincipalService>();
            services.AddTransient<IRequestIdService, RequestIdService>();
            services.AddTransient<IGraphServiceProvider, GraphServiceProvider>();
            services.AddSingleton<IPublicIpFromThirdPartyService, PublicIpFromThirdPartyService>();
            services.AddSingleton<IPublicIpService, PublicIpService>();
            services.AddScoped<IHealthService, HealthService>();

            //Data model services v2
            services.AddTransient<IStudyModelService, StudyModelService>();
            services.AddTransient<ISandboxModelService, SandboxModelService>();
            services.AddTransient<IPreApprovedDatasetModelService, PreApprovedDatasetModelService>();
            services.AddTransient<IStudySpecificDatasetModelService, StudySpecificDatasetModelService>();
            services.AddTransient<ISandboxDatasetModelService, SandboxDatasetModelService>();
            services.AddTransient<IResourceOperationModelService, ResourceOperationModelService>();

            //Domain Model Services
            services.AddTransient<IStudyReadService, StudyReadService>();
            services.AddTransient<IStudyCreateService, StudyCreateService>();
            services.AddTransient<IStudyUpdateService, StudyUpdateService>();
            services.AddTransient<IStudyDeleteService, StudyDeleteService>();
            services.AddTransient<IDatasetService, DatasetService>();
            services.AddTransient<ISandboxService, SandboxService>();
            services.AddTransient<ISandboxPhaseService, SandboxPhaseService>();
            services.AddTransient<ISandboxResourceReadService, SandboxResourceReadService>();
            services.AddTransient<IStudyDatasetService, StudyDatasetService>();
            services.AddTransient<IStudyParticipantLookupService, StudyParticipantLookupService>();
            services.AddTransient<IStudyParticipantCreateService, StudyParticipantCreateService>();
            services.AddTransient<IStudyParticipantRemoveService, StudyParticipantRemoveService>();
            services.AddTransient<ICloudResourceReadService, CloudResourceReadService>();
            services.AddTransient<ICloudResourceCreateService, CloudResourceCreateService>();
            services.AddTransient<ICloudResourceUpdateService, CloudResourceUpdateService>();
            services.AddTransient<ICloudResourceDeleteService, CloudResourceDeleteService>();
            services.AddTransient<ICloudResourceOperationCreateService, CloudResourceOperationCreateService>();
            services.AddTransient<ICloudResourceOperationReadService, CloudResourceOperationReadService>();
            services.AddTransient<ICloudResourceOperationUpdateService, CloudResourceOperationUpdateService>();

            services.AddTransient<IRegionService, RegionService>();
            services.AddScoped<IVariableService, VariableService>();
            services.AddTransient<ILookupService, LookupService>();

            //Provisioning service
            services.AddTransient<IProvisioningLogService, ProvisioningLogService>();
            services.AddTransient<ICloudResourceMonitoringService, CloudResourceMonitoringService>();
            services.AddTransient<IResourceProvisioningService, ResourceProvisioningService>();
            services.AddTransient<IRoleProvisioningService, RoleProvisioningService>();
            services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
            services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
            services.AddTransient<ICorsRuleProvisioningService, CorsRuleProvisioningService>();
            services.AddTransient<ICreateAndUpdateService, CreateAndUpdateService>();
            services.AddTransient<IDeleteOperationService, DeleteOperationService>();
            services.AddTransient<IFirewallService, FirewallService>();
            services.AddTransient<IOperationCheckService, OperationCheckService>();
            services.AddTransient<IOperationCompletedService, OperationCompletedService>();

            //Ext System Facade Services
            services.AddTransient<IRoleProvisioningService, RoleProvisioningService>();
            services.AddTransient<IDatasetFileService, DatasetFileService>();
            services.AddTransient<IStudyLogoService, StudyLogoService>();
            services.AddTransient<IStudySpecificDatasetService, StudySpecificDatasetService>();
            services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
            services.AddTransient<IResourceProvisioningService, ResourceProvisioningService>();
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

            //Azure Services
            services.AddTransient<IAzureResourceGroupService, AzureResourceGroupService>();
            services.AddTransient<IAzureNetworkSecurityGroupService, AzureNetworkSecurityGroupService>();
            services.AddTransient<IAzureBastionService, AzureBastionService>();
            services.AddTransient<IAzureVirtualNetworkService, AzureVirtualNetworkService>();
            services.AddTransient<IAzureVirtualMachineService, AzureVirtualMachineService>();
            services.AddTransient<IAzureVirtualMachineExtenedInfoService, AzureVirtualMachineExtendedInfoService>();
            services.AddTransient<IAzureQueueService, AzureQueueService>();
            services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();
            services.AddTransient<IAzureBlobStorageUriBuilderService, AzureBlobStorageUriBuilderService>();
            services.AddTransient<IAzureStorageAccountService, AzureStorageAccountService>();
            services.AddTransient<IAzureStorageAccountAccessKeyService, AzureStorageAccountAccessKeyService>();
            services.AddTransient<IAzureStorageAccountNetworkRuleService, AzureStorageAccountNetworkRuleService>();
            services.AddTransient<IAzureNetworkSecurityGroupRuleService, AzureNetworkSecurityGroupRuleService>();
            services.AddTransient<IAzureResourceSkuService, AzureResourceSkuService>();
            services.AddTransient<IAzureUserService, AzureUserService>();
            services.AddTransient<IAzureVirtualNetworkOperatingSystemService, AzureVirtualNetworkOperatingSystemService>();
            services.AddTransient<IAzureCostManagementService, AzureCostManagementService>();
            services.AddTransient<IAzureRoleAssignmentService, AzureRoleAssignmentService>();
            services.AddTransient<IAzureKeyVaultSecretService, AzureKeyVaultSecretService>();
        }

        void SetFileUploadLimits(IServiceCollection services)
        {
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = int.MaxValue;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
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
