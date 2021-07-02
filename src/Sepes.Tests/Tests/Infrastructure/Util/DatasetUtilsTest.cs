using Sepes.Common.Constants;
using Sepes.Common.Dto.Dataset;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using Xunit;

namespace Sepes.Tests.Util
{
    public class DatasetUtilsTest
    {
        [InlineData("Field Dataset.Name is required. Current value: ", "", "classification", "location")]
        [InlineData("Field Dataset.Classification is required. Current value: ", "Name", "", "location")]
        [InlineData("Field Dataset.Location is required. Current value: ", "Name", "classification", "")]
        [Theory]
        public void PerformUsualTestForPostedDatasets_ShouldReturnCorrectErrorMessage(string expectedResult, string name, string Classification, string Location)
        {
            var ex = Assert.Throws<ArgumentException>(() => DatasetUtils.PerformUsualTestForPostedDatasets(new DatasetCreateUpdateInputBaseDto { Name = name, Classification = Classification, Location = Location }));

            Assert.Equal(expectedResult, ex.Message);
        }

        [InlineData("Name", "classification", "location")]
        [Theory]
        public void PerformUsualTestForPostedDatasets_ShouldNotReturn(string name, string Classification, string Location)
        {
            DatasetUtils.PerformUsualTestForPostedDatasets(new DatasetCreateUpdateInputBaseDto { Name = name, Classification = Classification, Location = Location });
        }

        [Fact]
        public void GetStudySpecificStorageAccountResourceEntry_ShouldReturnCorrectResource()
        {
            var resources = new List<CloudResource>();
            resources.Add(new CloudResource { ResourceType = AzureResourceType.StorageAccount, Purpose = CloudResourcePurpose.StudySpecificDatasetStorageAccount });
            var result = DatasetUtils.GetStudySpecificStorageAccountResourceEntry(new Dataset{ StudySpecific = true, Resources = resources });

            Assert.Equal(AzureResourceType.StorageAccount, result.ResourceType);
        }

        [Fact]
        public void GetStudySpecificStorageAccountResourceEntry_ShouldReturnCorrectErrorMessage()
        {
            var expectedResult = "Dataset is empty";
            var ex = Assert.Throws<ArgumentException>(() => DatasetUtils.GetStudySpecificStorageAccountResourceEntry(null));
            Assert.Equal(expectedResult, ex.Message);
        }

        [Fact]
        public void GetStudyFromStudySpecificDatasetOrThrow_ShouldReturnCorrectErrorMessage()
        {
            var expectedResult = "Dataset is empty";
            var ex = Assert.Throws<ArgumentException>(() => DatasetUtils.GetStudyFromStudySpecificDatasetOrThrow(null));
            Assert.Equal(expectedResult, ex.Message);
        }

        [Fact]
        public void GetStudyFromStudySpecificDatasetOrThrow_ShouldReturnCorrectStudy()
        {
            var studyDatasets = new List<StudyDataset>();
            var study = new Study() { Name = "test", Id = 1 };
            studyDatasets.Add(new StudyDataset { Study = study });
            var dataset = new Dataset() { StudyDatasets = studyDatasets };
           var result = DatasetUtils.GetStudyFromStudySpecificDatasetOrThrow(dataset);
            Assert.Equal(study, result);
        }

        [Fact]
        public void GetStudyFromStudySpecificDatasetOrThrow_MissingInclude()
        {
            var expectedResult = "GetStudyFromStudySpecificDatasetOrThrow: Missing include on Study";
            var studyDatasets = new List<StudyDataset>();
            studyDatasets.Add(new StudyDataset { Study = null });
            var dataset = new Dataset() { StudyDatasets = studyDatasets };
            var ex = Assert.Throws<Exception>(() => DatasetUtils.GetStudyFromStudySpecificDatasetOrThrow(dataset));
            Assert.Equal(expectedResult, ex.Message);
        }

        [Fact]
        public void GetStudyFromStudySpecificDatasetOrThrow_NoReleationErrorMessage()
        {
            var expectedResult = "GetStudyFromStudySpecificDatasetOrThrow: Dataset appears to be study specific, but no relation found";
            var studyDatasets = new List<StudyDataset>();
            var dataset = new Dataset() { StudyDatasets = studyDatasets };
            var ex = Assert.Throws<Exception>(() => DatasetUtils.GetStudyFromStudySpecificDatasetOrThrow(dataset));
            Assert.Equal(expectedResult, ex.Message);
        }
    }
}
