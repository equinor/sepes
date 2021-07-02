using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Azure.Dto;
using Sepes.Azure.Service.Interface;
using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Common.Dto.Sandbox;
using Sepes.Common.Dto.Study;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Common.Constants;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Sepes.Tests.ModelFactory;
using Xunit;

namespace Sepes.Tests.Services.DomainServices
{
    public class StudyParticipantServiceTests : TestBaseWithInMemoryDb
    {
        public StudyParticipantServiceTests()
            : base()
        {

        }

        [Theory]
        [InlineData(1, "Sponsor Rep", ParticipantSource.Azure)]
        [InlineData(1, "Vendor Admin", ParticipantSource.Azure)]
        [InlineData(1, "Vendor Contributor", ParticipantSource.Azure)]
        [InlineData(1, "Study Viewer", ParticipantSource.Azure)]
        [InlineData(1, "Sponsor Rep", ParticipantSource.Db)]
        [InlineData(1, "Vendor Admin", ParticipantSource.Db)]
        [InlineData(1, "Vendor Contributor", ParticipantSource.Db)]
        [InlineData(1, "Study Viewer", ParticipantSource.Db)]
        public async Task AddingParticipant_ShouldSucceed(int studyId, string role, string source)
        {
            var createTask = SetupAndAddParticipantForSet(studyId, role, source);

            await createTask;

            var createdParticipant = createTask.Result;

            Assert.NotNull(createdParticipant);
            Assert.Equal(role, createdParticipant.Role);
        }


        [Theory]
        [InlineData(1, "Sponsor R1ep", ParticipantSource.Azure)]
        [InlineData(1, "Vendor Ad1min", ParticipantSource.Azure)]
        [InlineData(1, "Vendor Contr1ibutor", ParticipantSource.Azure)]
        [InlineData(1, "Study Vie1wer", ParticipantSource.Azure)]
        [InlineData(1, "Sponsor Re1p", ParticipantSource.Db)]
        [InlineData(1, "Vendor Ad1min", ParticipantSource.Db)]
        [InlineData(1, "Vendor Contr1ibutor", ParticipantSource.Db)]
        [InlineData(1, "Study Vie1wer", ParticipantSource.Db)]
        public async Task AddingParticipant_WithNonExistingRole_ShouldThrow(int studyId, string role, string source)
        {
            var createTask = SetupAndAddParticipantForSet(studyId, role, source);

            await Assert.ThrowsAsync<Exception>(() => createTask);
        }

        public async Task<StudyParticipantDto> SetupAndAddParticipantForSet(int studyId, string role, string source)
        {
            //SETUP          
            var userName = "newUserUsername";
            var userEmail = userName + "@somedomain.com";
            var userFullName = "Newly Added User";

            var participantToAdd = new ParticipantLookupDto() { DatabaseId = UserTestConstants.COMMON_NEW_PARTICIPANT_DB_ID, ObjectId = UserTestConstants.COMMON_NEW_PARTICIPANT_OBJECTID, EmailAddress = userEmail, FullName = userFullName, UserName = userName, Source = source };

            await RefreshAndPopulateTestDb();

            //GET REQUIRED SERVICES
            var db = _serviceProvider.GetService<SepesDbContext>();
            var mapper = _serviceProvider.GetService<IMapper>();
            var logger = _serviceProvider.GetService<ILogger<StudyParticipantCreateService>>();          

            var adUserServiceMock = new Mock<IAzureUserService>();
            adUserServiceMock.Setup(service => service.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new AzureUserDto() { DisplayName = userFullName, Mail = userEmail });

            //Study model service
            var studyModelService = StudyModelServiceMockFactory.StudyEfModelService(_serviceProvider);

            //Used to get current user
            var userServiceMock = GetUserServiceMock(UserTestConstants.COMMON_CUR_USER_DB_ID, UserTestConstants.COMMON_CUR_USER_OBJECTID);
            userServiceMock.Setup(service => service.GetByDbIdAsync(UserTestConstants.COMMON_NEW_PARTICIPANT_DB_ID)).ReturnsAsync(new UserDto() { Id = UserTestConstants.COMMON_NEW_PARTICIPANT_DB_ID, ObjectId = UserTestConstants.COMMON_NEW_PARTICIPANT_OBJECTID});
                      
            //Queue service mock 
            var queueServiceMock = new Mock<IProvisioningQueueService>();
            queueServiceMock.Setup(service => service.SendMessageAsync(It.IsAny<ProvisioningQueueParentDto>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ProvisioningQueueParentDto());

            var resourceReadServiceMock = new Mock<ICloudResourceReadService>();
            resourceReadServiceMock.Setup(service => service.GetDatasetResourceGroupIdsForStudy(It.IsAny<int>())).ReturnsAsync(new List<int> { 1 });
            resourceReadServiceMock.Setup(service => service.GetSandboxResourceGroupIdsForStudy(It.IsAny<int>())).ReturnsAsync(new List<int> { 2 });


            var operationCreateServiceMock = new Mock<ICloudResourceOperationCreateService>();           

            var operationUpdateServiceMock = new Mock<ICloudResourceOperationUpdateService>();

            var configuration = _serviceProvider.GetService<IConfiguration>();           

            var studyParticipantService = new StudyParticipantCreateService(db, mapper, logger, userServiceMock.Object, studyModelService, adUserServiceMock.Object, queueServiceMock.Object, resourceReadServiceMock.Object, operationCreateServiceMock.Object, operationUpdateServiceMock.Object);
            return await studyParticipantService.AddAsync(studyId, participantToAdd, role);
        }       

        Mock<IUserService> GetUserServiceMock(int id, string objectId = UserTestConstants.COMMON_CUR_USER_OBJECTID)
        {
            return UserServiceMockFactory.GetUserServiceMockForAdmin(id, objectId);
        }       

        async Task RefreshAndPopulateTestDb()
        {
            await ClearTestDatabase();
            var db = _serviceProvider.GetService<SepesDbContext>();

            StudyPopulator.Add(db, "Test Study 1", "Vendor for TS1", "WBS for TS1", UserTestConstants.COMMON_CUR_USER_DB_ID);

            StudyPopulator.Add(db, "Test Study 2", "Vendor for TS2", "WBS for TS2", UserTestConstants.COMMON_CUR_USER_DB_ID);

            StudyPopulator.Add(db, "Test Study 3", "Vendor for TS3", "WBS for TS3", UserTestConstants.COMMON_CUR_USER_DB_ID);

            db.SaveChanges();
        }
    }
}
