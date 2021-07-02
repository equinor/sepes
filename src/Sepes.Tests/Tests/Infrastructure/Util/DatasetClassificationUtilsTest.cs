using Sepes.Common.Constants;
using Sepes.Common.Dto.Sandbox;
using Sepes.Common.Model;
using Sepes.Common.Response.Sandbox;
using Sepes.Common.Util;
using System.Collections.Generic;
using Xunit;

namespace Sepes.Tests.Util
{
    public class DatasetClassificationUtilsTest
    {
        [Fact]
        public void SetRestrictionProperties_ShouldSetRestrictedDisplayText()
        {
            var list = new List<SandboxDatasetDto>();
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Open.ToString(), DatasetId = 1 });
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Internal.ToString(), DatasetId = 2 });
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Restricted.ToString(), DatasetId = 3 });
            var sandboxDetails = new SandboxDetails() { Datasets = list };
            DatasetClassificationUtils.SetRestrictionProperties(sandboxDetails);

            Assert.Equal(DatasetConstants.DATASET_RESTRICTION_TEXT_RESTRICTED, sandboxDetails.RestrictionDisplayText);
        }

        [Fact]
        public void SetRestrictionProperties_ShouldSetInternalDisplayText()
        {
            var list = new List<SandboxDatasetDto>();
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Open.ToString(), DatasetId = 1 });
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Internal.ToString(), DatasetId = 2 });
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Internal.ToString(), DatasetId = 3 });
            var sandboxDetails = new SandboxDetails() { Datasets = list };
            DatasetClassificationUtils.SetRestrictionProperties(sandboxDetails);

            Assert.Equal(DatasetConstants.DATASET_RESTRICTION_TEXT_INTERNAL, sandboxDetails.RestrictionDisplayText );
        }

        [Fact]
        public void SetRestrictionProperties_ShouldSetOpenDisplayText()
        {
            var list = new List<SandboxDatasetDto>();
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Open.ToString(), DatasetId = 1 });
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Open.ToString(), DatasetId = 2 });
            list.Add(new SandboxDatasetDto() { Classification = DatasetClassification.Open.ToString(), DatasetId = 3 });
            var sandboxDetails = new SandboxDetails() { Datasets = list };
            DatasetClassificationUtils.SetRestrictionProperties(sandboxDetails);

            Assert.Equal(DatasetConstants.DATASET_RESTRICTION_TEXT_OPEN, sandboxDetails.RestrictionDisplayText);
        }

        [Fact]
        public void SetRestrictionProperties_AvailableDatasets_ShouldSetRestrictedDisplayText()
        {
            var list = new List<AvailableDatasetItem>();

            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Open.ToString(), DatasetId = 1, AddedToSandbox = true });
            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Internal.ToString(), DatasetId = 2, AddedToSandbox = true });
            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Restricted.ToString(), DatasetId = 3, AddedToSandbox = true });

            AvailableDatasets availableDatasets = new AvailableDatasets(list);
            availableDatasets.Datasets = list;
            DatasetClassificationUtils.SetRestrictionProperties(availableDatasets);

            Assert.Equal(DatasetConstants.DATASET_RESTRICTION_TEXT_RESTRICTED, availableDatasets.RestrictionDisplayText);
        }

        [Fact]
        public void SetRestrictionProperties_AvailableDatasets_ShouldSetInternalDisplayText()
        {
            var list = new List<AvailableDatasetItem>();

            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Open.ToString(), DatasetId = 1, AddedToSandbox = true });
            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Internal.ToString(), DatasetId = 2, AddedToSandbox = true });
            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Internal.ToString(), DatasetId = 3, AddedToSandbox = true });

            AvailableDatasets availableDatasets = new AvailableDatasets(list);
            availableDatasets.Datasets = list;
            DatasetClassificationUtils.SetRestrictionProperties(availableDatasets);

            Assert.Equal(DatasetConstants.DATASET_RESTRICTION_TEXT_INTERNAL, availableDatasets.RestrictionDisplayText);
        }

        [Fact]
        public void SetRestrictionProperties_AvailableDatasets_ShouldSetOpendDisplayText()
        {
            var list = new List<AvailableDatasetItem>();

            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Open.ToString(), DatasetId = 1, AddedToSandbox = true });
            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Open.ToString(), DatasetId = 2, AddedToSandbox = true });
            list.Add(new AvailableDatasetItem() { Classification = DatasetClassification.Open.ToString(), DatasetId = 3, AddedToSandbox = true });

            AvailableDatasets availableDatasets = new AvailableDatasets(list);
            availableDatasets.Datasets = list;

            DatasetClassificationUtils.SetRestrictionProperties(availableDatasets);

            Assert.Equal(DatasetConstants.DATASET_RESTRICTION_TEXT_OPEN, availableDatasets.RestrictionDisplayText);
        }
    }
}
