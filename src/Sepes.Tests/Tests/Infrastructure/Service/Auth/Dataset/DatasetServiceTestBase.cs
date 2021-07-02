using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Tests.Tests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Tests.Services
{
    public class DatasetServiceTestBase : TestBaseWithInMemoryDb
    {

        public DatasetServiceTestBase()
            : base()
        {
         
        }

        protected Study CreateTestStudy(int id)
        {
            return new Study()
            {
                Id = id,
                Name = "TestStudy",
                Vendor = "Bouvet",
                WbsCode = "1234.1345afg",
                StudyParticipants = new List<StudyParticipant>()
            };
        }

        protected List<Study> CreateTestStudyList(int id)
        {
            return new List<Study>() { CreateTestStudy(id) };
        }


        protected async Task<SepesDbContext> RefreshAndSeedTestDatabase(int datasetId)
        {
            var db = await ClearTestDatabase();           

            var dataset = CreateTestDataset(datasetId);

            db.Datasets.Add(dataset);
            await db.SaveChangesAsync();
            return db;
        }

        protected Dataset CreateTestDataset(int datasetId)
        {
           return new Dataset()
            {
                Id = datasetId,
                Name = "TestDataset",
                Description = "For Testing",
                Location = "Norway West",
                Classification = "Internal",
                StorageAccountName = "testdataset",
                LRAId = 1337,
                DataId = 420,
                SourceSystem = "SAP",
                CountryOfOrigin = "Norway"
            };
        }

        protected List<Dataset> CreateTestDatasetList(int datasetId)
        {
            return new List<Dataset>() { CreateTestDataset(datasetId) };
        }
        }
}
