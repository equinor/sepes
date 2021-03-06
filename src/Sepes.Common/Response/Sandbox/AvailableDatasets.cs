﻿using System.Collections.Generic;

namespace Sepes.Common.Response.Sandbox
{
    public class AvailableDatasets
    {
        public string Classification { get; set; }
        public string RestrictionDisplayText { get; set; }
        public IEnumerable<AvailableDatasetItem> Datasets { get; set; }

        public AvailableDatasets(IEnumerable<AvailableDatasetItem> availableDatasets)
        {
            Datasets = availableDatasets;
        }

        public AvailableDatasets()
        {
            
        }
    }  
}
