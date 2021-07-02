using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Common.ServiceMockFactories.Infrastructure;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{

    public static class DatasetServiceMockFactory
    {
        public static IPreApprovedDatasetModelService GetPreApprovedDatasetModelService(ServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var db = serviceProvider.GetService<SepesDbContext>();         
            var logger = serviceProvider.GetService<ILogger<PreApprovedDatasetModelService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);

            var studyPermissionService = StudyPermissionServiceMockFactory.Create(serviceProvider, userService.Object);

            var operationPermissionService = OperationPermissionServiceMockFactory.Create(userService.Object);

            var service = new PreApprovedDatasetModelService(config, db, logger, studyPermissionService, operationPermissionService);
            return service;
        }

        public static Mock<IPreApprovedDatasetModelService> GetPreApprovedDatasetModelService(List<Dataset> datasets)
        {
            var studyModelServiceMock = new Mock<IPreApprovedDatasetModelService>();
            studyModelServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<UserOperation>())).ReturnsAsync((int a, UserOperation b) => datasets.FirstOrDefault(s => s.Id == a));
            return studyModelServiceMock;
        }

        public static Mock<IStudySpecificDatasetModelService> GetStudySpecificDatasetModelService(List<Dataset> datasets)
        {
            var studyModelServiceMock = new Mock<IStudySpecificDatasetModelService>();
            studyModelServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<UserOperation>())).ReturnsAsync((int a, UserOperation b) => datasets != null ? datasets.FirstOrDefault(s => s.Id == a) : null);
            studyModelServiceMock.Setup(x => x.GetForResourceAndFirewall(It.IsAny<int>(), It.IsAny<UserOperation>())).ReturnsAsync((int a, UserOperation b) => datasets != null ? datasets.FirstOrDefault(s => s.Id == a) : null);
            studyModelServiceMock.Setup(x => x.GetByIdWithoutPermissionCheckAsync(It.IsAny<int>())).ReturnsAsync((int a) => datasets != null ? datasets.FirstOrDefault(s => s.Id == a) : null);
            return studyModelServiceMock;
        }

        public static IDatasetService GetDatasetService(ServiceProvider serviceProvider, List<Dataset> datasets = null)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<DatasetService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);

            var studyPermissionService = StudyPermissionServiceMockFactory.Create(serviceProvider, userService.Object);

            var preApprovedDatasetModelService = GetPreApprovedDatasetModelService(datasets);

            return new DatasetService(db, mapper, logger, userService.Object, studyPermissionService, preApprovedDatasetModelService.Object);
        }

        public static IStudyDatasetService GetStudyDatasetService(ServiceProvider serviceProvider, List<Study> studies = null, List<Dataset> datasets = null)
        {
            var db = serviceProvider.GetService<SepesDbContext>();          
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<StudyDatasetService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);            

            var studyModelServiceMock = new Mock<IStudyEfModelService>();
            studyModelServiceMock.Setup(x => x.GetForDatasetsAsync(It.IsAny<int>(), It.IsAny<UserOperation>())).ReturnsAsync(( int a, UserOperation b) => studies != null ? studies.FirstOrDefault(s=> s.Id == a) : null);

            var studyPermissionService = StudyPermissionServiceMockFactory.Create(serviceProvider, userService.Object);

            var studySpecificDatasetModelService = GetStudySpecificDatasetModelService(datasets);

            return new StudyDatasetService(db, mapper, logger, userService.Object, studyPermissionService, studyModelServiceMock.Object, studySpecificDatasetModelService.Object);          
        }

        public static IDatasetFirewallService GetStudyDatasetFirewallService(ServiceProvider serviceProvider, string serverIp)
        {           
            var logger = serviceProvider.GetService<ILogger<DatasetFirewallService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);
            var ipService = PublicIpServiceMockFactory.CreateSucceedingService(serviceProvider, serverIp);

            return new DatasetFirewallService(logger, userService.Object, ipService);
        }

        public static IStudySpecificDatasetService GetStudySpecificDatasetService(ServiceProvider serviceProvider, List<Study> studies = null, List<Dataset> datasets = null)
        {
            if(studies == null)
            {
                studies = new List<Study>();
            }

            var db = serviceProvider.GetService<SepesDbContext>();
            var config = serviceProvider.GetService<IConfiguration>();
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<StudySpecificDatasetService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);

            var studyPermissionService = StudyPermissionServiceMockFactory.Create(serviceProvider, userService.Object);

            var studyModelServiceMock = new Mock<IStudyEfModelService>();
            studyModelServiceMock.Setup(x => x.GetForDatasetsAsync(It.IsAny<int>(), It.IsAny<UserOperation>())).ReturnsAsync((int a, UserOperation b) => studies != null ? studies.FirstOrDefault(s => s.Id == a) : null);
            studyModelServiceMock.Setup(x => x.GetForDatasetCreationAsync(It.IsAny<int>(), It.IsAny<UserOperation>())).ReturnsAsync((int a, UserOperation b) => studies != null ? studies.FirstOrDefault(s => s.Id == a) : null);
            
            var dsCloudResourceServiceMock = new Mock<IDatasetCloudResourceService>();
            dsCloudResourceServiceMock.Setup(x => x.CreateResourcesForStudySpecificDatasetAsync(It.IsAny<Study>(), It.IsAny<Dataset>(), "192.168.1.1", default(CancellationToken))).Returns(default(Task));

            var studySpecificDatasetModelService = GetStudySpecificDatasetModelService(datasets);

            var studyWbsValidationService = StudyWbsValidationMockServiceFactory.GetService(serviceProvider, true, false);

            return new StudySpecificDatasetService(db, mapper, logger, userService.Object, studyPermissionService,  studyModelServiceMock.Object, studyWbsValidationService, studySpecificDatasetModelService.Object, dsCloudResourceServiceMock.Object);
        }
    }
}
