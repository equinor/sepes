using System.Collections.Generic;
using Sepes.Common.Dto.Dataset;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services
{
    public class DatasetServiceTests : DatasetServiceTestBase
    {   

        public DatasetServiceTests()
            :base()
        {
    
        }          


        [Fact]
        public async void GetDatasetsLookupAsync_ShouldReturnDatasets_IfExists()
        {
           await  RefreshAndSeedTestDatabase(5);
            var datasetService = DatasetServiceMockFactory.GetDatasetService(_serviceProvider);

            IEnumerable<DatasetLookupItemDto> result = await datasetService.GetLookupAsync();
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetDatasetsAsync_ShouldReturnDatasets_IfExists()
        {
            await RefreshAndSeedTestDatabase(5);
            var datasetService = DatasetServiceMockFactory.GetDatasetService(_serviceProvider);

            IEnumerable<DatasetDto> result = await datasetService.GetAllAsync();
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetDatasetByIdAsync_ShouldReturnDataset_IfExists()
        {
            var datasetId = 10;
            _ = await ClearTestDatabase();
            var datasets = CreateTestDatasetList(datasetId);
       
            var datasetService = DatasetServiceMockFactory.GetDatasetService(_serviceProvider, datasets);

            DatasetDto result = await datasetService.GetByIdAsync(datasetId);
            Assert.NotNull(result);
        }

            
    }
}
