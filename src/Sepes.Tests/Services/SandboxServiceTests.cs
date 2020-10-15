using Microsoft.Extensions.DependencyInjection;
using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sepes.Tests.Services
{
    public class SandboxServiceTests
    {
        public ServiceCollection Services { get; private set; }
        public ServiceProvider ServiceProvider { get; protected set; }

        public SandboxServiceTests()
        {
            Services = BasicServiceCollectionFactory.GetServiceCollectionWithInMemory();
            Services.AddTransient<IStudyService, StudyService>();
            Services.AddTransient<ISandboxService, SandboxService>();
            ServiceProvider = Services.BuildServiceProvider();
        }

        void RefreshTestDb()
        {
            var db = ServiceProvider.GetService<SepesDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        async Task<StudyDto> AddStudyToTestDatabase(int studyId)
        {
            var studyService = ServiceProvider.GetService<IStudyService>();
            StudyCreateDto study = new StudyCreateDto()
            {       
                Name = "TestStudy",
                Vendor = "Bouvet",
                WbsCode = "1234.1345afg"
            };

            return await studyService.CreateStudyAsync(study);
        }

        [Fact]
        public async void GetSandboxesByStudyIdAsync_ShouldReturnSandboxes()
        {
            RefreshTestDb();
            ISandboxService sandboxService = ServiceProvider.GetService<ISandboxService>();
            int studyId = 1;
            await AddStudyToTestDatabase(studyId);

            var sandbox = new SandboxCreateDto() { Name = "TestSandbox" };
            var sandbox2 = new SandboxCreateDto() { Name = "TestSandbox2" };
            _ = await sandboxService.CreateAsync(studyId, sandbox);
            _ = await sandboxService.CreateAsync(studyId, sandbox2);

            var sandboxes = await sandboxService.GetSandboxesForStudyAsync(studyId);

            Assert.NotEmpty(sandboxes);
            Assert.Equal(2, sandboxes.Count());

        }

      

        [Fact]
        public async void AddSandboxToStudyAsync_ShouldAddSandbox()
        {
            RefreshTestDb();
            
            int studyId = 1;
            var newlyCreatedSandbox = await CreateAndGetSandbox(studyId);         

            Assert.NotNull(newlyCreatedSandbox);
            Assert.NotNull(newlyCreatedSandbox.Resources);
            Assert.Equal(6, newlyCreatedSandbox.Resources.Count); //Resource group, network, nsg, diag stor, bastion

            //Resource group test
            var sandboxResourceGroup = newlyCreatedSandbox.Resources.FirstOrDefault(o=> o.Type == AzureResourceType.ResourceGroup);         
            Assert.NotNull(sandboxResourceGroup);      
            Assert.Equal("Success", sandboxResourceGroup.LastKnownProvisioningState);

            //Diag storage account
            var diagStorageAccount = newlyCreatedSandbox.Resources.FirstOrDefault(o => o.Type == AzureResourceType.StorageAccount);
            CommonTestsForScheduledResources(diagStorageAccount);

            //VNet resource and operation created
            var vNet = newlyCreatedSandbox.Resources.FirstOrDefault(o => o.Type == AzureResourceType.VirtualNetwork);
            CommonTestsForScheduledResources(vNet);

            //NSG resource and operation created
            var nsg = newlyCreatedSandbox.Resources.FirstOrDefault(o => o.Type == AzureResourceType.NetworkSecurityGroup);
            CommonTestsForScheduledResources(nsg);
        }

        void CommonTestsForScheduledResources(SandboxResourceLightDto dto)
        {
            Assert.NotNull(dto);
            Assert.Null(dto.LastKnownProvisioningState);
            Assert.Null(dto.Status);      
        }

        [Fact]
        public async void SetUpSandboxResources_ShouldSucceed()
        {
            RefreshTestDb();

            int studyId = 1;
            var newlyCreatedSandbox = await CreateAndGetSandbox(studyId);

            var provisioningService = ServiceProvider.GetService<ISandboxResourceProvisioningService>();

            
            await provisioningService.DequeueWorkAndPerformIfAny();


            //Call the method that picks up work
        }

        async Task CreateSandbox(int studyId)
        {
            RefreshTestDb();
            var sandboxService = ServiceProvider.GetService<ISandboxService>();

            await AddStudyToTestDatabase(studyId);

            var sandboxCreateDto = new SandboxCreateDto() { Name = "TestSandbox", Region = "norwayeast" };
            _ = await sandboxService.CreateAsync(studyId, sandboxCreateDto);
        }

        async Task<SandboxDto> CreateAndGetSandbox(int studyId)
        {
            var sandboxService = ServiceProvider.GetService<ISandboxService>();

            await CreateSandbox(studyId);
            var sandboxesForStydy = await sandboxService.GetSandboxesForStudyAsync(studyId);
            var newlyCreatedSandbox = sandboxesForStydy.FirstOrDefault();

            return newlyCreatedSandbox;
        }

        [Theory]
        [InlineData("TestSandbox", "")]
        [InlineData("TestSandbox", null)]
        public async void AddSandboxToStudyAsync_ToStudyWithoutWbs_ShouldFail(string name, string wbs)
        {
            RefreshTestDb();
            IStudyService studyService = ServiceProvider.GetService<IStudyService>();
            ISandboxService sandboxService = ServiceProvider.GetService<ISandboxService>();

            StudyCreateDto study = new StudyCreateDto()
            {
                Name = "TestStudy",
                Vendor = "Bouvet",
                WbsCode = wbs
            };

            StudyDto createdStudy = await studyService.CreateStudyAsync(study);
            int studyID = (int)createdStudy.Id;

            var sandbox = new SandboxCreateDto() { Name = name };

            await Assert.ThrowsAsync<ArgumentException>(() => sandboxService.CreateAsync(studyID, sandbox));
        }

        [Fact]
        public async void RemoveSandboxFromStudyAsync_ShouldRemoveSandbox()
        {
            RefreshTestDb();
            ISandboxService sandboxService = ServiceProvider.GetService<ISandboxService>();
            int studyId = 1;
            await AddStudyToTestDatabase(studyId);

            var sandbox = new SandboxCreateDto() { Name = "TestSandbox" };
            _ = await sandboxService.CreateAsync(studyId, sandbox);
            var sandboxFromDb = await sandboxService.GetSandboxesForStudyAsync(studyId);
            var sandboxId = (int)sandboxFromDb.First().Id;
            _ = await sandboxService.DeleteAsync(studyId, sandboxId);

            var sandboxes = await sandboxService.GetSandboxesForStudyAsync(studyId);

            Assert.Empty(sandboxes);
        }

        [Theory]
        [InlineData(1, 420)]
        [InlineData(420, 1)]
        public async void RemoveSandboxFromStudyAsync_ShouldThrow_IfSandboxOrStudyDoesNotExist(int providedStudyId, int providedSandboxId)
        {
            RefreshTestDb();
            ISandboxService sandboxService = ServiceProvider.GetService<ISandboxService>();
            int studyId = 1;
            await AddStudyToTestDatabase(studyId);

            var sandbox = new SandboxCreateDto() { Name = "TestSandbox" };
            _ = await sandboxService.CreateAsync(studyId, sandbox);

            await Assert.ThrowsAsync<NotFoundException>(() => sandboxService.DeleteAsync(providedStudyId, providedSandboxId));
        }
    }
}
