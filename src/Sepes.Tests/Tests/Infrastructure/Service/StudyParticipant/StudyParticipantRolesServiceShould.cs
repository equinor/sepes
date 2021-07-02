using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Common.Constants;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services.DomainServices.Lookup
{
    public class StudyParticipantRolesServiceShould : TestBaseWithInMemoryDb
    {
        public StudyParticipantRolesServiceShould()
           : base()
        {
        }

        [Theory]
        [InlineData(StudyRoles.SponsorRep, StudyRoles.VendorAdmin, StudyRoles.VendorContributor, StudyRoles.StudyViewer)]
        public async void ReturnRelevantRolesForAdmin(params string[] expectedRoles)
        {
            await RefreshAndSeedTestDatabase();
            var expectedAsDto = expectedRoles.Select(r => new LookupDto(r, r)).ToList();
            var participantRolesService = StudyParticipantRolesServiceMockFactory.GetForAdmin(_serviceProvider);

            await ActAndAssert(participantRolesService, expectedAsDto);
        }

        [Fact]      
        public async void ReturnNoRolesForSponsor_IfNoStudySpecificRole()
        {
            await RefreshAndSeedTestDatabase();
            var participantRolesService = StudyParticipantRolesServiceMockFactory.GetForSponsor(_serviceProvider);

            await ActAndAssert(participantRolesService, new List<LookupDto>());
        }

        [Fact]
        public async void ReturnNoRolesForDatasetAdmin_IfNoStudySpecificRole()
        {
            await RefreshAndSeedTestDatabase();
            var participantRolesService = StudyParticipantRolesServiceMockFactory.GetForDatasetAdmin(_serviceProvider);

            await ActAndAssert(participantRolesService, new List<LookupDto>());
        }

        [Fact]
        public async void ReturnNoRolesForEmployee_IfNoStudySpecificRole()
        {
            await RefreshAndSeedTestDatabase();
            var participantRolesService = StudyParticipantRolesServiceMockFactory.GetForEmployee(_serviceProvider);

            await ActAndAssert(participantRolesService, new List<LookupDto>());
        }

        [Theory]
        [MemberData(nameof(AvailableRolesForStudySpecificRoles))]
        public async void ReturnRelevantRolesForStudyRoles(string studyRole, List<LookupDto> expectedRoles)
        {
            await RefreshAndSeedTestDatabase(studyRole);
            var participantRolesService = StudyParticipantRolesServiceMockFactory.GetForBasicUser(_serviceProvider);

           await ActAndAssert(participantRolesService, expectedRoles);
        }

        async Task ActAndAssert(IStudyParticipantRolesService studyParticipantRolesService, List<LookupDto> expectedRoles)
        {
            var rolesFromService = (await studyParticipantRolesService.RolesAvailableForUser(StudyTestConstants.CREATED_BY_ME_ID)).ToList();

            CollectionAssert.AreEquivalent(expectedRoles, rolesFromService);
            CollectionAssert.AllItemsAreUnique(rolesFromService);
        }

        public static List<LookupDto> AvailableRolesForAdmin() => new List<LookupDto>
            {
                new LookupDto { Key = StudyRoles.StudyViewer, DisplayValue = StudyRoles.StudyViewer },
                new LookupDto { Key = StudyRoles.SponsorRep, DisplayValue = StudyRoles.SponsorRep },
                new LookupDto { Key = StudyRoles.VendorAdmin, DisplayValue = StudyRoles.VendorAdmin },
                new LookupDto { Key = StudyRoles.VendorContributor, DisplayValue = StudyRoles.VendorContributor }
            };

        public static List<object[]> AvailableRolesForStudySpecificRoles()
        {
            var assignableByStudyOwnerAndSponsorRep = new List<LookupDto>
            {
                new LookupDto { Key = StudyRoles.StudyViewer, DisplayValue = StudyRoles.StudyViewer },
                new LookupDto { Key = StudyRoles.SponsorRep, DisplayValue = StudyRoles.SponsorRep },
                new LookupDto { Key = StudyRoles.VendorAdmin, DisplayValue = StudyRoles.VendorAdmin },
                new LookupDto { Key = StudyRoles.VendorContributor, DisplayValue = StudyRoles.VendorContributor }
            };

            var assignableByVendorAdmin = new List<LookupDto>
            {
                new LookupDto { Key = StudyRoles.VendorAdmin, DisplayValue = StudyRoles.VendorAdmin },
                new LookupDto { Key = StudyRoles.VendorContributor, DisplayValue = StudyRoles.VendorContributor },             
            };

            var assignableByVendorContributor = new List<LookupDto>
            {
                new LookupDto { Key = StudyRoles.VendorContributor, DisplayValue = StudyRoles.VendorContributor }
            };

            var allData = new List<object[]>
            {
                new object[] { StudyRoles.StudyViewer, new List<LookupDto> { }},
                new object[] { StudyRoles.StudyOwner, assignableByStudyOwnerAndSponsorRep},
                new object[] { StudyRoles.SponsorRep, assignableByStudyOwnerAndSponsorRep},
                new object[] { StudyRoles.VendorAdmin, assignableByVendorAdmin},
                new object[] { StudyRoles.VendorContributor, assignableByVendorContributor }
            };

            return allData;
        }

        async Task<SepesDbContext> RefreshAndSeedTestDatabase(string roleType = null)
        {
            var db = await ClearTestDatabaseAddUser();          

            var study = new Study()
            {
                Id = StudyTestConstants.CREATED_BY_ME_ID,
                Name = "Test Study with specific participants",
                StudyParticipants = new List<StudyParticipant>()
                  
            };

            if (string.IsNullOrWhiteSpace(roleType))
            {
                study.StudyParticipants.Add(new StudyParticipant() { UserId = UserTestConstants.COMMON_ALTERNATIVE_STUDY_OWNER_DB_ID, RoleName = StudyRoles.StudyOwner });
            }
            else
            {
                study.StudyParticipants.Add(new StudyParticipant() { UserId = UserTestConstants.COMMON_CUR_USER_DB_ID, RoleName = roleType });            
            }

            db.Studies.Add(study);

            await db.SaveChangesAsync();
            return db;
        }
    }
}


