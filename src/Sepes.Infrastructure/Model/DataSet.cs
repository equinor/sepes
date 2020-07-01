﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sepes.Infrastructure.Model
{
    public class Dataset : UpdateableBaseModel
    {
        [MaxLength(64)]
        [Required]
        public string Name { get; set; }

        [MaxLength(64)]
        //[Required]
        public string Location { get; set; }

        [MaxLength(32)]
        //[Required]
        public string Classification { get; set; }

        public int LRAId { get; set; }
        public int DataId { get; set; }
        public string SourceSystem { get; set; }
        public string BADataOwner { get; set; }
        public string Asset { get; set; }
        public string CountryOfOrigin { get; set; }
        public string AreaL1 { get; set; }
        public string AreaL2 { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }

        // Attributes used for linking.
        // ------------------------------
        public ICollection<StudyDataset> StudyDatasets { get; set; }

        //StudyID is only populated if dataset is StudySpecific.
        //This is accounted for in API calls.
        public int? StudyNo { get; set; }
    }
}
