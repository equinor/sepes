using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Azure.Service.Interface;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public class VirtualMachineMockServiceFactory
    {     

        public static IVirtualMachineCreateService GetVirtualMachineCreateService(ServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();
            var config = serviceProvider.GetService<IConfiguration>();
            var logger = serviceProvider.GetService<ILogger<VirtualMachineCreateService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);

            var sandboxModelServiceMock = new Mock<ISandboxModelService>();            

            var resourceCreateService = new Mock<ICloudResourceCreateService>();

            var resourceUpdateService = new Mock<ICloudResourceUpdateService>();

            var resourceDeleteService = new Mock<ICloudResourceDeleteService>();

            var resourceReadService = new Mock<ICloudResourceReadService>();

            var provisioningQueueServiceMock = new Mock<IProvisioningQueueService>();

            var azureKeyVaultSecretServiceMock = new Mock<IAzureKeyVaultSecretService>();

            var virtualMachineOperatingSystemService = new Mock<IVirtualMachineOperatingSystemService>();

            return new VirtualMachineCreateService(config, db, logger, mapper, userService.Object,
                sandboxModelServiceMock.Object,            
                resourceCreateService.Object,
                  resourceReadService.Object,
                resourceUpdateService.Object,
                resourceDeleteService.Object,              
                provisioningQueueServiceMock.Object,
                azureKeyVaultSecretServiceMock.Object,
                virtualMachineOperatingSystemService.Object);
        }

        public static IVirtualMachineValidationService GetVirtualMachineValidationService()
        { 
            return new VirtualMachineValidationService();
        }

        public static IVirtualMachineSizeService GetVirtualMachineSizeService(ServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);

            var sandboxModelServiceMock = new Mock<ISandboxModelService>();
            sandboxModelServiceMock.Setup(x => x.GetRegionByIdAsync(1, It.IsAny<UserOperation>())).ReturnsAsync("norwayeast");

            return new VirtualMachineSizeService(db, mapper, sandboxModelServiceMock.Object);
        }
    }
}
