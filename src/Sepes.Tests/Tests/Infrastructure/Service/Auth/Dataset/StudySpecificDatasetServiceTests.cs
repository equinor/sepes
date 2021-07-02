using Sepes.Common.Dto.Dataset;
using Sepes.Common.Exceptions;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services
{
    public class StudySpecificDatasetServiceTests : DatasetServiceTestBase
    {

        public StudySpecificDatasetServiceTests()
            : base()
        {

        }


        [Fact]
        public async void CreateStudySpecificDataset_WhenStudyIsMissingWbs_ShouldFail()
        {
            _ = await ClearTestDatabase();
            var studyId = 1;
            var studies = CreateTestStudyList(studyId);
            studies[0].WbsCode = null;

            var datasetService = DatasetServiceMockFactory.GetStudySpecificDatasetService(_serviceProvider, studies);
            await Assert.ThrowsAsync<InvalidWbsException>(() => datasetService.CreateStudySpecificDatasetAsync(1, new DatasetCreateUpdateInputBaseDto() { Name = "testds", Location = "norwayeast", Classification = "open" }, "192.168.1.1"));
        }
    }
}
