//using System.Threading.Tasks;
//using Sepes.Common.Constants;
//using Sepes.Common.Dto.Sandbox;
//using Sepes.Tests.Setup;
//using Xunit;

//namespace Sepes.Tests.Services.Infrastructure.Sandbox
//{
//    public class SandboxServiceShould : ServiceTestBase
//    {
//        [Theory]
//        [InlineData(AppRoles.Admin, SandboxPhase.Open)]
//        public async Task MoveSandbox_WithNoPhases_ToNextPhase_ShouldSucceed(string appRole, SandboxPhase currentPhase)
//        {
//            var db = await ClearTestDatabaseAddUser();

//            var sandboxId = 1;

//            SandboxPopulator.AddWithoutPhases(db, sandboxId, "hasNoPhases");

//            var sandboxService = SandboxServiceWithMocksFactory.Create(_serviceProvider, appRole, UserConstants.COMMON_CUR_USER_DB_ID, db);

//            await sandboxService.MoveToNextPhaseAsync(sandboxId, default(CancellationToken));

//            var sandbox = await sandboxService.GetSandboxDetailsAsync(sandboxId);

//            Assert.NotNull(sandbox);
//            Assert.Equal(SandboxPhase.DataAvailable, sandbox.CurrentPhase);
//        }
//    }
//}