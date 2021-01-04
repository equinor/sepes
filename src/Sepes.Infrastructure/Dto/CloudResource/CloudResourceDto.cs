﻿using Sepes.Infrastructure.Dto.Sandbox;
using System;
using System.Collections.Generic;

namespace Sepes.Infrastructure.Dto
{
    public class CloudResourceDto : UpdateableBaseDto
    {
        public int SandboxId { get; set; }

        public string StudyName { get; set; }
        public string SandboxName { get; set; }

        public string ResourceId { get; set; }

        public string ResourceKey { get; set; }

        public string ResourceName { get; set; }

        public string ResourceType { get; set; }

        public string ResourceGroupId { get; set; }

        public string ResourceGroupName { get; set; }

        public string Status { get; set; }

        public string ProvisioningState { get; set; }

        public DateTime? Deleted { get; set; }

        public string DeletedBy { get; set; }

        public string ConfigString { get; set; }

        public bool SandboxControlled { get; set; }

        public Dictionary<string, string> Tags
        {
            get;set;
        }

        public string Region { get; set; }

        public List<CloudResourceOperationDto> Operations { get; set; }
    }
}