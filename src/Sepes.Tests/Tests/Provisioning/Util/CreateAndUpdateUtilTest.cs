using Sepes.Common.Constants.CloudResource;
using Sepes.Common.Dto;
using Sepes.Provisioning.Service.Interface;
using Sepes.Test.Common.ServiceMockFactories;
using System;
using Xunit;

namespace Sepes.Tests.Util.Provisioning
{
    public class CreateAndUpdateUtilTest
    {
        readonly ICreateAndUpdateService _createAndUpdateService;

        public CreateAndUpdateUtilTest()
        {
            _createAndUpdateService = CreateAndUpdateServiceMock.CreateBasic();
        }

        [Fact]
        public void WillBeHandledAsCreateOrUpdate_ShouldReturnFalse()
        {
            var result = _createAndUpdateService.CanHandle(new CloudResourceOperationDto {  });

            Assert.False(result);
        }

        [Fact]
        public void WillBeHandledAsCreateOrUpdate_ShouldReturnException()
        {
            var ex = Assert.Throws<ArgumentException>(() => _createAndUpdateService.CanHandle(null));
            Assert.Equal("Cloud-Resource-Operation was null", ex.Message);
        }

        [Fact]
        public void WillBeHandledAsCreateOrUpdate_ShouldReturnTrue2()
        {
            var result = _createAndUpdateService.CanHandle(new CloudResourceOperationDto { OperationType = CloudResourceOperationType.CREATE });

            Assert.True(result);
        }

        [Fact]
        public void WillBeHandledAsCreateOrUpdate_ShouldReturnTrue3()
        {
            var result = _createAndUpdateService.CanHandle(new CloudResourceOperationDto { OperationType = CloudResourceOperationType.UPDATE });

            Assert.True(result);
        }
    }
}
