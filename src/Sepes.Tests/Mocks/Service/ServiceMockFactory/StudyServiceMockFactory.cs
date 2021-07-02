using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public static class StudyServiceMockFactory
    {
       
        public static IStudyCreateService CreateService(ServiceProvider serviceProvider, bool wbsValidationSucceeds = true)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();         
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);
            var operationPermissionFactory = OperationPermissionServiceMockFactory.Create(userService.Object);

            var studyModelService = StudyModelServiceMockFactory.StudyEfModelService(serviceProvider);
           
            var logoCreateServiceMock = new Mock<IStudyLogoCreateService>();
            var logoReadServiceMock = new Mock<IStudyLogoReadService>();

            var studyWbsValidationService = GetStudyWbsValidationServiceMock(wbsValidationSucceeds);           

            var dsCloudResourceServiceMock = new Mock<IDatasetCloudResourceService>();
            dsCloudResourceServiceMock.Setup(x => x.CreateResourceGroupForStudySpecificDatasetsAsync(It.IsAny<Study>(), default)).Returns(Task.CompletedTask);

            return new StudyCreateService(db, mapper, userService.Object, studyModelService, logoCreateServiceMock.Object, operationPermissionFactory, dsCloudResourceServiceMock.Object, studyWbsValidationService.Object);
        }

        public static Mock<IStudyWbsValidationService> GetStudyWbsValidationServiceMock(bool wbsValidationSucceeds)
        {
            var studyWbsValidationService = new Mock<IStudyWbsValidationService>();

            if (wbsValidationSucceeds)
            {
                studyWbsValidationService.Setup(x => x.ValidateForStudyCreate(It.IsAny<Study>()))
                    .Callback<Study>(s => { s.WbsCodeValid = wbsValidationSucceeds; s.WbsCodeValidatedAt = DateTime.UtcNow; });
            }
            else
            {
                studyWbsValidationService.Setup(x => x.ValidateForStudyCreate(It.IsAny<Study>())).ThrowsAsync(new InvalidWbsException("message", "userMessage"));
            }

            return studyWbsValidationService;
        }

        public static IStudyDeleteService DeleteService(ServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<SepesDbContext>();         
            var logger = serviceProvider.GetService<ILogger<StudyDeleteService>>();
            var userService = UserServiceMockFactory.GetUserServiceMockForAdmin(1);

            var studyEfModelService = StudyModelServiceMockFactory.StudyEfModelService(serviceProvider);

            var studySpecificDatasetService = DatasetServiceMockFactory.GetStudySpecificDatasetService(serviceProvider);

            var logoReadServiceMock = new Mock<IStudyLogoReadService>();
            var logoDeleteServiceMock = new Mock<IStudyLogoDeleteService>();

            var resourceReadServiceMock = new Mock<ICloudResourceReadService>();
            //Todo: add some real resources to delete
            resourceReadServiceMock.Setup(service => service.GetSandboxResourcesForDeletion(It.IsAny<int>())).ReturnsAsync(new List<CloudResource>());

            return new StudyDeleteService(
                logger,
                db,
                userService.Object,
                studyEfModelService,              
                logoDeleteServiceMock.Object,
                studySpecificDatasetService,
                resourceReadServiceMock.Object);
        }
    }
}
