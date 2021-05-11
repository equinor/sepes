﻿using Sepes.Common.Dto.Dataset;
using Sepes.Common.Dto.Interfaces;
using Sepes.Infrastructure.Interface;
using Sepes.Infrastructure.Response.Sandbox;
using System.Collections.Generic;

namespace Sepes.Common.Dto.Study
{
    public class StudyDetailsDto : UpdateableBaseDto, IHasLogoUrl, IHasStudyParticipants
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string WbsCode { get; set; }

        public string Vendor { get; set; }

        public bool Restricted { get; set; }

        public string LogoUrl { get; set; }

        public StudyPermissionsDto Permissions { get; set; } = new StudyPermissionsDto();

        public List<DatasetListItemDto> Datasets { get; set; }

        public List<SandboxListItem> Sandboxes { get; set; }

        public List<StudyParticipantDto> Participants { get; set; }
        
    }
}
