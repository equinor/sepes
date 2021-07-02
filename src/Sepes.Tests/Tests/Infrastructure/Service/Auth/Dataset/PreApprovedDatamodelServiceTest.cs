using Sepes.Common.Constants;
using Sepes.Common.Exceptions;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services
{
    public class PreApprovedDatamodelServiceTest : DatasetServiceTestBase
    {    

        public PreApprovedDatamodelServiceTest()
            :base()
        {
    
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(1337)]
        public async void GetDatasetByIdAsync_ShouldThrow_IfDoesNotExist(int id)
        {
            var datasetId = 1;

            _ = await ClearTestDatabase();
            var datasets = CreateTestDatasetList(datasetId);

            var preApprovedDatasetModelService = DatasetServiceMockFactory.GetPreApprovedDatasetModelService(_serviceProvider);
           
            await Assert.ThrowsAsync<NotFoundException>(async () => await preApprovedDatasetModelService.GetByIdAsync(id, UserOperation.Study_Read));
        }
    }
}
