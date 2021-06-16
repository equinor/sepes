﻿using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Tests.Setup
{
    public static class StudyServiceMockFactory
    {
        public static IStudyListModelService StudyListModelService(ServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();         
            var logger = serviceProvider.GetService<ILogger<StudyListModelService>>();           
            var userService = UserFactory.GetUserServiceMockForAdmin(1);
            var studyPermissionService = StudyPermissionService(serviceProvider);

            return new StudyListModelService(config, logger, userService.Object, studyPermissionService);
        }

        public static IStudyEfModelService StudyEfModelService(ServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var db = serviceProvider.GetService<SepesDbContext>();
            var logger = serviceProvider.GetService<ILogger<StudyEfModelService>>();
            var userService = UserFactory.GetUserServiceMockForAdmin(1);

            var studyPermissionService = StudyPermissionService(serviceProvider);

            return new StudyEfModelService(config, db, logger, userService.Object, studyPermissionService);
        }            

        public static IStudyCreateService CreateService(ServiceProvider serviceProvider, bool wbsValidationSucceeds = true)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<StudyCreateService>>();
            var userService = UserFactory.GetUserServiceMockForAdmin(1);

            var studyModelService = StudyEfModelService(serviceProvider);
           
            var logoCreateServiceMock = new Mock<IStudyLogoCreateService>();
            var logoReadServiceMock = new Mock<IStudyLogoReadService>();

            var studyWbsValidationService = GetStudyWbsValidationServiceMock(wbsValidationSucceeds);           

            var dsCloudResourceServiceMock = new Mock<IDatasetCloudResourceService>();
            dsCloudResourceServiceMock.Setup(x => x.CreateResourceGroupForStudySpecificDatasetsAsync(It.IsAny<Study>(), default(CancellationToken))).Returns(Task.CompletedTask);

            return new StudyCreateService(db, mapper, logger, userService.Object, studyModelService, logoCreateServiceMock.Object, logoReadServiceMock.Object, dsCloudResourceServiceMock.Object, studyWbsValidationService.Object);
        }

        static Mock<IStudyWbsValidationService> GetStudyWbsValidationServiceMock(bool wbsValidationSucceeds)
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

        public static IStudyUpdateService UpdateService(ServiceProvider serviceProvider, bool wbsValidationSucceeds = true)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<StudyUpdateService>>();
            var userService = UserFactory.GetUserServiceMockForAdmin(1);

            var studyModelService = StudyEfModelService(serviceProvider);

            var logoReadServiceMock = new Mock<IStudyLogoReadService>();
            var logoCreateServiceMock = new Mock<IStudyLogoCreateService>();
            var logoDeleteServiceMock = new Mock<IStudyLogoDeleteService>();

            var studyWbsValidationService = GetStudyWbsValidationServiceMock(wbsValidationSucceeds);

            return new StudyUpdateService(db, mapper, logger, userService.Object, studyModelService, logoReadServiceMock.Object, logoCreateServiceMock.Object, logoDeleteServiceMock.Object , studyWbsValidationService.Object);
        }

        public static IStudyPermissionService StudyPermissionService(ServiceProvider serviceProvider, IUserService userService = null) {

            var mapper = serviceProvider.GetService<IMapper>();            

            return new StudyPermissionService(mapper, userService == null ? UserFactory.GetUserServiceMockForAdmin(1).Object : userService);
        }

        public static IStudyDeleteService DeleteService(ServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetService<SepesDbContext>();
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<StudyDeleteService>>();
            var userService = UserFactory.GetUserServiceMockForAdmin(1);

            var studyEfModelService = StudyEfModelService(serviceProvider);

            var studySpecificDatasetService = DatasetServiceMockFactory.GetStudySpecificDatasetService(serviceProvider);

            var logoReadServiceMock = new Mock<IStudyLogoReadService>();
            var logoDeleteServiceMock = new Mock<IStudyLogoDeleteService>();

            var resourceReadServiceMock = new Mock<ICloudResourceReadService>();
            //Todo: add some real resources to delete
            resourceReadServiceMock.Setup(service => service.GetSandboxResourcesForDeletion(It.IsAny<int>())).ReturnsAsync(new List<CloudResource>());

            return new StudyDeleteService(db, mapper, logger, userService.Object,
                studyEfModelService,
                logoReadServiceMock.Object,
                logoDeleteServiceMock.Object,
                studySpecificDatasetService,
                resourceReadServiceMock.Object);
        }
    }
}
