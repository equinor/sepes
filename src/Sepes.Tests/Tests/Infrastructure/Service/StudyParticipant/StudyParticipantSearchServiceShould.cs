using Sepes.Tests.Setup;
using System.Linq;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services.DomainServices
{
    public class StudyParticipantSearchServiceShould : StudyParticipantSearchTestBase
    {
        public StudyParticipantSearchServiceShould()
           : base()
        {

        }

        [Fact]
        public async void ServeRelevantHits_BasedOnName()
        {
            await RefreshAndSeedTestDatabase();
            var studyParticipantLookupService = StudyParticipantMockFactory.GetStudyParticipantLookupService(_serviceProvider);
            var usersWithName = await studyParticipantLookupService.SearchAsync("John");
            
            Assert.Equal(2, usersWithName.Count());
        }

        [Fact]
        public async void ServeRelevantHits_BasedOnEmail()
        {
            await RefreshAndSeedTestDatabase();
            var studyParticipantLookupService = StudyParticipantMockFactory.GetStudyParticipantLookupService(_serviceProvider);
            var usersWithName = await studyParticipantLookupService.SearchAsync("John@hotmail.com");
            Assert.Single(usersWithName);
        }

        [Fact]
        public async void ServeNoHits_WhenWrongName()
        {
            await RefreshAndSeedTestDatabase();
            var studyParticipantLookupService = StudyParticipantMockFactory.GetStudyParticipantLookupService(_serviceProvider);
            var usersWithName = await studyParticipantLookupService.SearchAsync("No Person Has this name");
            Assert.Empty(usersWithName);
        }
    }
}
