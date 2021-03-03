﻿using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Dto.Sandbox;
using Sepes.Infrastructure.Interface;
using Sepes.Infrastructure.Model;
using System.Collections.Generic;

namespace Sepes.Infrastructure.Response.Sandbox
{
    public class SandboxDetails : UpdateableBaseDto, IHasCurrentPhase
    {
        public string Name { get; set; }

        public int StudyId { get; set; }

        public string StudyName { get; set; }

        public string Region { get; set; }

        public string TechnicalContactName { get; set; }

        public string TechnicalContactEmail { get; set; }
        public string LinkToCostAnalysis { get; set; }

        public SandboxPhase CurrentPhase { get; set; }

        public bool Deleted { get; set; }

        public bool ReadyForPhaseChange { get; set; }

        public string RestrictionDisplayText { get; set; }

        public List<SandboxDatasetDto> Datasets { get; set; }

        public SandboxPermissions Permissions { get; set; } = new SandboxPermissions();
    }
}
