﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sepes.Infrastructure.Model
{
    public class SandboxResourceOperation : UpdateableBaseModel
    {        
        public int SandboxResourceId { get; set; }

        public int? DependsOnOperationId { get; set; }

        [MaxLength(32)]
        public string OperationType { get; set; }

        public string Status { get; set; }

        public int TryCount { get; set; }

        public int MaxTryCount { get; set; }

        public string BatchId { get; set; }

        public string CreatedBySessionId { get; set; }

        public string CarriedOutBySessionId { get; set; }

        public string Description { get; set; }

        public string LatestError { get; set; }

        [MaxLength(64)]
        public string QueueMessageId { get; set; }
        [MaxLength(64)]
        public string QueueMessagePopReceipt{ get; set; }

        public DateTime? QueueMessageVisibleAgainAt { get; set; }

        public SandboxResource Resource { get; set; }
        
        public SandboxResourceOperation DependsOnOperation { get; set; }

        public ICollection<SandboxResourceOperation> DependantOnThisOperation { get; set; }
    }
}
