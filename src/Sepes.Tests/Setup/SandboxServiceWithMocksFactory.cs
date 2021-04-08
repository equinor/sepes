﻿using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;

namespace Sepes.Tests.Setup
{

    public static class SandboxServiceWithMocksFactory
    {
        public static ISandboxService Create(ServiceProvider serviceProvider, string userAppRole, int userId, SepesDbContext db = null)
        {
            if(db == null)
            {
                db = serviceProvider.GetService<SepesDbContext>();
            }

            var config = serviceProvider.GetService<IConfiguration>();
          
            var mapper = serviceProvider.GetService<IMapper>();
            var logger = serviceProvider.GetService<ILogger<SandboxService>>();

            var userService = UserFactory.GetUserServiceMockForAppRole(userAppRole, userId);

            //Study model service
            var studyModelService = StudyServiceMockFactory.StudyModelService(serviceProvider);

            var sandboxModelServiceMock = new Mock<ISandboxModelService>();

            var sandboxResourceCreateServiceMock = new Mock<ISandboxResourceCreateService>();
            var sandboxResourceDeleteServiceMock = new Mock<ISandboxResourceDeleteService>();

            return new SandboxService(config, db, mapper, logger, userService.Object, studyModelService, sandboxModelServiceMock.Object, sandboxResourceCreateServiceMock.Object, sandboxResourceDeleteServiceMock.Object);
        }      
    }
}
