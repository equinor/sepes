﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sepes.RestApi.Services;
using System.Diagnostics.CodeAnalysis;
using Sepes.Infrastructure.Model.Context;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Sepes.Infrastructure.Model.Config;
using AutoMapper;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Model.Automapper;
using Sepes.RestApi.Middelware;

namespace Sepes.RestApi
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        readonly IWebHostEnvironment _env;
        readonly ILogger _logger;
        readonly IConfiguration _configuration;
        //readonly IConfigService _configService;

        public Startup(IWebHostEnvironment env, ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;

            var logMsg = "Sepes Startup Constructor";
            Trace.WriteLine(logMsg);
            _logger.LogWarning(logMsg);

            _configuration = configuration;           
        }

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var logMsg = "ConfigureServices starting";
            Trace.WriteLine(logMsg);
            _logger.LogWarning(logMsg);

            var azureAdConfig = new AzureAdOptions();
            _configuration.GetSection("AzureAd").Bind(azureAdConfig);

            var sepesAzureConfig = new SepesAzureOptions();
            _configuration.Bind(sepesAzureConfig);

            // The following line enables Application Insights telemetry collection.
            // If this is left empty then no logs are made. Unknown if still affects performance.
            Trace.WriteLine("Configuring Application Insights");
            services.AddApplicationInsightsTelemetry(_configuration[ConfigConstants.APPI_KEY]);

            DoMigration();

            var enableSensitiveDataLogging = true;

            var readWriteDbConnectionString = _configuration[ConfigConstants.DB_READ_WRITE_CONNECTION_STRING];

            services.AddDbContext<SepesDbContext>(
              options => options.UseSqlServer(
                  readWriteDbConnectionString,
                  assembly => assembly.MigrationsAssembly(typeof(SepesDbContext).Assembly.FullName))
              .EnableSensitiveDataLogging(enableSensitiveDataLogging)
              );

            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddAzureAdBearer(options => _configuration.Bind(options));

            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    //builder.WithOrigins("http://example.com", "http://www.contoso.com");
                    // Issue: 39  replace with above commented code. Preferably add config support for the URLs. 
                    // Perhaps an if to check if environment is running in development so we can still easily debug without changing code
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
            services.AddAutoMapper(typeof(AutoMappingConfigs));

            //var mappingConfig = new MapperConfiguration(mc =>
            //{
            //    mc.AddProfile(new AutoMappingConfigs());
            //});
            // var azureService = new AzureService(_configuration);
            // var dbService = new SepesDb(readWriteDbConnectionString);
            // var podService = new PodService(azureService);
            //var studyService = new StudyService(dbService, podService);
            //studyService.LoadStudies();

            //services.AddSingleton<ISepesDb>(dbService);
            //services.AddSingleton<IAuthService>(new AuthService(_configService.AuthConfig));
            //services.AddSingleton<IAzureService>(azureService);
            //services.AddSingleton<IPodService>(podService);
            //services.AddSingleton<IStudyService_OLD>(studyService);
            services.AddTransient<IStudyService, Infrastructure.Service.StudyService>();

            var logMsgDone = "Configuring services done";
            Trace.WriteLine(logMsgDone);
            _logger.LogWarning(logMsgDone);
        }

        void DoMigration()
        {
            var disableMigrations = _configuration[ConfigConstants.DISABLE_MIGRATIONS];

            var logMessage = "";

            if (!String.IsNullOrWhiteSpace(disableMigrations) && disableMigrations.ToLower() == "false")
            {
                logMessage = "Migrations are disabled and will be skipped!";
            
            }
            else
            {
                logMessage = "Performing database migrations";
            }
           
            Trace.WriteLine(logMessage);
            _logger.LogWarning(logMessage);

            string sqlConnectionStringOwner = _configuration[ConfigConstants.DB_OWNER_CONNECTION_STRING];

            if (string.IsNullOrEmpty(sqlConnectionStringOwner))
            {
                throw new Exception("Could not obtain database OWNER connection string. Unable to run migrations");
            }            

            DbContextOptionsBuilder<SepesDbContext> createDbOptions = new DbContextOptionsBuilder<SepesDbContext>();
            createDbOptions.UseSqlServer(sqlConnectionStringOwner);

            using (var ctx = new SepesDbContext(createDbOptions.Options))
            {
                ctx.Database.SetCommandTimeout(300);
                ctx.Database.Migrate();
            }

            var logMsgDone = "Do migration done";

            Trace.WriteLine(logMsgDone);
            _logger.LogWarning(logMsgDone);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var logMsg = "Configure";

            Trace.WriteLine(logMsg);
            _logger.LogWarning(logMsg);


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

            var httpOnlyRaw = _configuration["HttpOnly"];

            // UseHttpsRedirection doesn't work well with docker.        
            if (!String.IsNullOrWhiteSpace(httpOnlyRaw) && httpOnlyRaw.ToLower() == "true")
            {
                var logMsgHttps = "Using HTTP only";
                Trace.WriteLine(logMsgHttps);
                _logger.LogWarning(logMsgHttps);
            }
            else
            {
                var logMsgHttps = "Also using HTTPS. Activating https redirection";
                Trace.WriteLine(logMsgHttps);
                _logger.LogWarning(logMsgHttps);
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var logMsgDone = "Configure done";

            Trace.WriteLine(logMsgDone);
            _logger.LogWarning(logMsgDone);
        }
    }
}
