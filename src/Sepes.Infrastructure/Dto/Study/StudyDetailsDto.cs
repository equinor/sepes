﻿using Sepes.Infrastructure.Dto.Interfaces;
using Sepes.Infrastructure.Dto.Sandbox;
using Sepes.Infrastructure.Interface;
using System.Collections.Generic;

namespace Sepes.Infrastructure.Dto.Study
{
    public class StudyDetailsDto : UpdateableBaseDto, IHasLogoUrl, IHasStudyParticipants
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string ResultsAndLearnings { get; set; }

        public string WbsCode { get; set; }

        public string Vendor { get; set; }

        public bool Restricted { get; set; }

        public string LogoUrl { get; set; }

        public StudyPermissionsDto Permissions { get; set; } = new StudyPermissionsDto();

        public ICollection<DatasetDto> Datasets { get; set; }

        public ICollection<SandboxDto> Sandboxes { get; set; }

        public List<StudyParticipantDto> Participants { get; set; }
        
    }
}