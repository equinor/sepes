using System.Threading.Tasks;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services
{
    public class StudyDatasetServiceTests : DatasetServiceTestBase
    {     

        public StudyDatasetServiceTests()
            :base()
        {
          
        }    

        async Task<Study> RefreshAndAddStudyToTestDb(int studyId, int datasetId)
        {
            var db = await RefreshAndSeedTestDatabase(datasetId);

            var study = CreateTestStudy(studyId);
            db.Studies.Add(study);

            await db.SaveChangesAsync();
            
            return study;
        }


        [Theory]
        [InlineData(1337, 1)]
        [InlineData(1, 1337)]
        [InlineData(1337, 1337)]
        public async void AddDatasetToStudyAsync_ShouldThrow_IfDatasetOrStudyDoesNotExist(int providedStudyId, int providedDatasetId)
        {
            var studies = CreateTestStudyList(1);          
           _ = await ClearTestDatabase();          

            var studyDatasetService = DatasetServiceMockFactory.GetStudyDatasetService(_serviceProvider, studies);

            await Assert.ThrowsAsync<NotFoundException>(() => studyDatasetService.AddPreApprovedDatasetToStudyAsync(providedStudyId, providedDatasetId));
        }

        [Fact]
        public async void AddDatasetFromStudyAsync_ShouldAddDataset()
        {
            var studyId = 1;
            var studies = CreateTestStudyList(studyId);

            var datasetId = 1;
            var datasets = CreateTestDatasetList(datasetId);
            _ = await ClearTestDatabase();

            var studyDatasetService = DatasetServiceMockFactory.GetStudyDatasetService(_serviceProvider, studies, datasets);

            await studyDatasetService.AddPreApprovedDatasetToStudyAsync(studyId, datasetId);
            var dataset = await studyDatasetService.GetDatasetByStudyIdAndDatasetIdAsync(studyId, datasetId);

            Assert.NotNull(dataset);
        }

        [Theory]
        [InlineData(1337, 1)]
        [InlineData(1, 1337)]
        [InlineData(1337, 1337)]
        public async void RemoveDatasetFromStudyAsync_ShouldThrow_IfDatasetOrStudyDoesNotExist(int providedStudyId, int providedDatasetId)
        {           
            _ = await ClearTestDatabase();

            var studyDatasetService = DatasetServiceMockFactory.GetStudyDatasetService(_serviceProvider);
            var datasetService = DatasetServiceMockFactory.GetStudyDatasetService(_serviceProvider);
           
            await Assert.ThrowsAsync<NotFoundException>(() => datasetService.RemovePreApprovedDatasetFromStudyAsync(providedStudyId, providedDatasetId));
        }

        [Fact]
        public async void RemoveDatasetFromStudyAsync_ShouldRemoveDataset()
        {
            var studyId = 1;
            var studies = CreateTestStudyList(studyId);

            var datasetId = 1;
            var datasets = CreateTestDatasetList(datasetId);
            _ = await ClearTestDatabase();

            var datasetService = DatasetServiceMockFactory.GetStudyDatasetService(_serviceProvider, studies, datasets);

            await datasetService.AddPreApprovedDatasetToStudyAsync(studyId, datasetId);
            await datasetService.RemovePreApprovedDatasetFromStudyAsync(studyId, datasetId);
            await Assert.ThrowsAsync<NotFoundException>(() => datasetService.GetDatasetByStudyIdAndDatasetIdAsync(studyId, datasetId));
        }

    }
}
