﻿using System;

namespace Sepes.Infrastructure.Dto.Sandbox
{
    public class SandboxResourceOperationDto : UpdateableBaseDto
    {
        public string Description { get; set; }
        public string OperationType { get; set; }

        public string Status { get; set; }

        public int TryCount { get; set; }

        public int MaxTryCount { get; set; }

        public string BatchId { get; set; }

        public string CreatedBySessionId { get; set; }

        public string CarriedOutBySessionId { get; set; }

        public int? DependsOnOperationId { get; set; }
    

        public SandboxResourceOperationDto DependsOnOperation { get; set; }


        public SandboxResourceDto Resource { get; set; }
    }
}
