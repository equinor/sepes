﻿using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;

namespace Sepes.Infrastructure.Dto
{
    public class AzureSandboxDto
    {
        public string StudyName { get; set; }

        public string SandboxName { get; set; }

        public string ResourceGroupId { get { return ResourceGroup.Id; } }

        public string ResourceGroupName { get { return ResourceGroup.Name; } }

        public IResourceGroup ResourceGroup { get; set; }

        public IStorageAccount DiagnosticsStorage { get; set; }

        public INetworkSecurityGroup NetworkSecurityGroup { get; set; }

        public AzureVNetDto VNet { get; set; }
    }
}
