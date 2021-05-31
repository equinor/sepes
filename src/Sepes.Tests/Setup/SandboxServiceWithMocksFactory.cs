﻿using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Sepes.Tests.Setup
{

    public static class SandboxServiceWithMocksFactory
    {
        public static ISandboxService ForSandboxCreate(ServiceProvider serviceProvider, string userAppRole, int userId, IStudyWbsValidationService studyWbsValidationService, List<Study> studies = null, List<Sandbox> sandboxForSandboxDetails = null)
        {
            //STUDY MODEL SERVICE
            var studyModelServiceMock = new Mock<IStudyModelService>();
            
            studyModelServiceMock
                .Setup(x => 
                    x.GetForSandboxCreateAndDeleteAsync(It.IsAny<int>(), It.IsAny<UserOperation>()))
                .ReturnsAsync((int a, UserOperation b) => studies?.FirstOrDefault(s => s.Id == a));

            //SANDBOX MODEL SERVICE
            var sandboxModelServiceMock = new Mock<ISandboxModelService>();          

            return Create(serviceProvider, userAppRole, userId, studyModelServiceMock.Object, sandboxModelServiceMock.Object, studyWbsValidationService);
        }
        
        public static ISandboxService Create(ServiceProvider serviceProvider, string userAppRole, int userId, IStudyModelService studyModelService, ISandboxModelService sandboxModelService, IStudyWbsValidationService studyWbsValidationServiceMock)
        {
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<SandboxService>>();

            var userService = UserFactory.GetUserServiceMockForAppRole(userAppRole, userId);

            var sandboxResourceCreateServiceMock = new Mock<ISandboxResourceCreateService>();
            var sandboxResourceDeleteServiceMock = new Mock<ISandboxResourceDeleteService>();

            return new SandboxService(mapper, logger, userService.Object, sandboxModelService, studyModelService, studyWbsValidationServiceMock, sandboxResourceCreateServiceMock.Object, sandboxResourceDeleteServiceMock.Object);
        }      
    }
}
