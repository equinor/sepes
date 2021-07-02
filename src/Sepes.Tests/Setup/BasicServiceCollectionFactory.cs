using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sepes.Common.Constants;
using Sepes.Common.Interface;
using Sepes.Infrastructure.Automapper;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Provisioning.Service;
using Sepes.Provisioning.Service.Interface;
using Sepes.Tests.Common.Mocks.Service;

namespace Sepes.Tests.Setup
{
    public static class BasicServiceCollectionFactory
    {
        public static ServiceCollection GetServiceCollectionWithInMemory()
        {
            var services = new ServiceCollection();


            services.AddDbContext<SepesDbContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);
            IConfiguration config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", optional: true)
             .AddJsonFile("appsettings.Development.json", optional: true)
             .AddEnvironmentVariables().Build();

            //config.GetSection("ConnectionStrings").Bind(new ConnectionStrings());
            services.AddSingleton<IConfiguration>(config);
           

            if (!string.IsNullOrWhiteSpace(config[ConfigConstants.APPI_KEY]))
            {    //https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service         
                services.AddApplicationInsightsTelemetryWorkerService(config[ConfigConstants.APPI_KEY]);
            }

            services.AddLogging();            
            
            services.AddAutoMapper(typeof(AutoMappingConfigs));
            services.AddTransient<CloudResourceOperationReadService>();            
            services.AddTransient<IRequestIdService, RequestIdServiceMock>();        

            //Sepes Services           
            services.AddTransient<IVariableService, VariableService>();
            services.AddTransient<IStudyListService, StudyListService>();

            services.AddTransient<IDapperQueryService, DapperQueryService>();                   
                     

            //Resource provisioning services
            services.AddSingleton<IProvisioningQueueService, ProvisioningQueueService>();
            services.AddTransient<ICloudResourceOperationReadService, CloudResourceOperationReadService>();
            services.AddTransient<IResourceProvisioningService, ResourceProvisioningService>();         

            return services;
        } 
    } 
}
