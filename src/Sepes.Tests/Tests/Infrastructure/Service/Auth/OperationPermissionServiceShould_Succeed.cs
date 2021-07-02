using Moq;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System.Threading.Tasks;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services.DomainServices
{
    public partial class OperationPermissionServiceShould : TestBase
    {
        [Theory]     
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task SucceedOn_StudyCreate_IfCorrectPermissions(bool admin, bool sponsor)
        {
            var userService = GetUserServiceMock(admin: admin, sponsor: sponsor);
            await PerformSucceedingTestSingleOperation(userService, UserOperation.Study_Create);           
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task SucceedOn_StudyCreateOrUpdate_IfCorrectPermissions(bool admin, bool sponsor)
        {
            var userService = GetUserServiceMock(admin: admin, sponsor: sponsor);
            await PerformSucceedingTestMultipleOperation(userService, UserOperation.Study_Create, UserOperation.Study_Update_Metadata);        
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, true)]

        public async Task SucceedOn_StudyDelete_IfCorrectPermissions(bool employee, bool admin)
        {
            var userService = GetUserServiceMock(employee: employee, admin: admin);
            await PerformSucceedingTestSingleOperation(userService, UserOperation.Study_Delete);
        }

        [Fact]

        public async Task SucceedOn_ReadPreApprovedDataset_IfCorrectPermissions()
        {
            var userService = GetUserServiceMock(employee: true);
            await PerformSucceedingTestSingleOperation(userService, UserOperation.PreApprovedDataset_Read);
        }


        [Theory]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]

        public async Task SucceedOn_CreateOrUpdatePreApprovedDataset_IfCorrectPermissions(bool employee, bool admin, bool datasetAdmin)
        {
            var userService = GetUserServiceMock(employee: employee, admin: admin, datasetAdmin: datasetAdmin);
            await PerformSucceedingTestSingleOperation(userService, UserOperation.PreApprovedDataset_Create_Update_Delete);
        }

        async Task PerformSucceedingTestSingleOperation(Mock<IUserService> userService, UserOperation userOperation)
        {
            var operationPermissionService = OperationPermissionServiceMockFactory.Create(userService.Object);

            await PerformBoolCheck(operationPermissionService, userOperation, true);

            await operationPermissionService.HasAccessToOperationOrThrow(userOperation);
        }

        async Task PerformSucceedingTestMultipleOperation(Mock<IUserService> userService, params UserOperation[] userOperations)
        {
            var operationPermissionService = OperationPermissionServiceMockFactory.Create(userService.Object);

            foreach(var curOperation in userOperations)
            {
                await PerformBoolCheck(operationPermissionService, curOperation, true);
            }          

            await operationPermissionService.HasAccessToAnyOperationOrThrow(userOperations);
        }

        async Task PerformBoolCheck(IOperationPermissionService operationPermissionService, UserOperation userOperation, bool expected)
        {
            var allowed = await operationPermissionService.HasAccessToOperation(userOperation);
            Assert.Equal(expected, allowed);           
        }
    }
}
