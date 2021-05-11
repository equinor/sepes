﻿using Moq;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Provisioning;
using Sepes.Infrastructure.Service.Azure.Interface;
using Sepes.Tests.Common.ServiceMockFactories.Azure;
using System.Threading;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class AzureNetworkSecurityGroupMockServiceFactory
    {
        public static Mock<IAzureNetworkSecurityGroupService> CreateBasicForCreate() {

            var mock = new Mock<IAzureNetworkSecurityGroupService>();          
            mock.Setup(us => us.EnsureCreated(It.IsAny<ResourceProvisioningParameters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ResourceProvisioningParameters ipt, CancellationToken cancellation) => ProvisioningResultFactory.Create(ipt, AzureResourceType.NetworkSecurityGroup));

            return mock;

        }
    }
}
