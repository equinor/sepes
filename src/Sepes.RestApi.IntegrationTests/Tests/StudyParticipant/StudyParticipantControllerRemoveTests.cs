using Sepes.Common.Constants;
using Sepes.RestApi.IntegrationTests.TestHelpers.Requests;
using Sepes.RestApi.IntegrationTests.Setup;
using Sepes.RestApi.IntegrationTests.TestHelpers.AssertSets;
using Sepes.Tests.Common.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Sepes.RestApi.IntegrationTests.Tests
{
    [Collection("Integration tests collection")]
    public class StudyParticipantControllerRemoveTests : ControllerTestBase
    {
        public StudyParticipantControllerRemoveTests(TestHostFixture testHostFixture)
            : base(testHostFixture)
        {

        }


        [Theory]
        [InlineData(false, true, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, true, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)] 
        [InlineData(false, false, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, false, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        public async void RemovingStudyParticipant_AsOwner_HavingRelevantAppRole_ShouldSucceeed(bool restrictedStudy, bool isAdmin, bool isSponsor, params string[] rolesToRemove)
        {
            await WithBasicSeeds();

            foreach(var curRoleToRemove in rolesToRemove)
            {
                var study = await WithStudyCreatedByCurrentUser(restrictedStudy, rolesForOtherUser: new List<string> { curRoleToRemove });

                SetScenario(isAdmin: isAdmin, isSponsor: isSponsor);

                await PerformTestsExpectSuccess(study.Id, curRoleToRemove);
            }
           
        }

        [Theory]
        [InlineData(false, false, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(false, false, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(false, true, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(false, true, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, false, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, false, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, true, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, true, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        public async void RemovingStudyParticipantWithoutRelevantRoles_ShouldThrow(bool studyCreatedByCurrentUser, bool restrictedStudy, bool employee, params string[] rolesToRemove)
        {
            await WithBasicSeeds();

            foreach (var curRoleToRemove in rolesToRemove)
            {
                var study = studyCreatedByCurrentUser ? await WithStudyCreatedByCurrentUser(restrictedStudy, rolesForOtherUser: new List<string> { curRoleToRemove }) : await WithStudyCreatedByOtherUser(restrictedStudy, rolesForOtherUser: new List<string> { curRoleToRemove });

                SetScenario(isEmployee: employee);

                await PerformTestsExpectFailure(study.Id, curRoleToRemove);
            }
        }


        [Theory]
        [InlineData(false, false, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(false, true, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, false, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        [InlineData(true, true, StudyRoles.VendorAdmin, StudyRoles.VendorContributor)]
        public async void VendorAdmin_RemovingVendorRoles_ShouldSucceed(bool employee, bool restrictedStudy, params string[] rolesToRemove)
        {
            await WithBasicSeeds();

            foreach (var curRoleToRemove in rolesToRemove)
            {               
                var study = await WithStudyCreatedByOtherUser(restrictedStudy, new List<string> { StudyRoles.VendorAdmin }, rolesForOtherUser: new List<string> { curRoleToRemove });
                SetScenario(isEmployee: employee);
                await PerformTestsExpectSuccess(study.Id, curRoleToRemove);
            }
        }

        [Theory]
        [InlineData(false, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep)]
        [InlineData(false, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep)]
        [InlineData(true, false, StudyRoles.StudyViewer, StudyRoles.SponsorRep)]
        [InlineData(true, true, StudyRoles.StudyViewer, StudyRoles.SponsorRep)]
        public async void VendorAdmin_RemovingNonVendorRoles_ShouldThrow(bool employee, bool restrictedStudy, params string[] rolesToRemove)
        {
            await WithBasicSeeds();

            foreach (var curRoleToRemove in rolesToRemove)
            {
                var study = await WithStudyCreatedByOtherUser(restrictedStudy, additionalRolesForCurrentUser: new List<string> { StudyRoles.VendorAdmin }, rolesForOtherUser: new List<string> { curRoleToRemove });
                SetScenario(isEmployee: employee);
                await PerformTestsExpectFailure(study.Id, curRoleToRemove);
            }
        }      

        async Task PerformTestsExpectSuccess(int studyId, string roleToRemove)
        {          
            var studyParticipantRemoveConversation = await StudyParticipantAdderAndRemover.RemoveAndExpectSuccess(_restHelper, studyId, UserTestConstants.COMMON_NEW_PARTICIPANT_DB_ID, roleToRemove);
            ApiResponseBasicAsserts.ExpectNoContent(studyParticipantRemoveConversation.Response);          
        }

        async Task PerformTestsExpectFailure(int studyId, string roleToRemove)
        {
            var studyParticipantRemoveConversation = await StudyParticipantAdderAndRemover.RemoveAndExpectFailure(_restHelper, studyId, UserTestConstants.COMMON_NEW_PARTICIPANT_DB_ID, roleToRemove);
            ApiResponseBasicAsserts.ExpectForbiddenWithMessage(studyParticipantRemoveConversation.Response);
        }
    }
}
