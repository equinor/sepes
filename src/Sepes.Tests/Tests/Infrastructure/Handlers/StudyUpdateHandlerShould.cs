using Sepes.Common.Dto.Study;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using System;
using Sepes.Tests.Mocks.HandlerMockFactory;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Infrastructure.Handlers
{
    public class StudyUpdateHandlerShould : TestBaseWithInMemoryDb
    {
        public StudyUpdateHandlerShould()
            :base()
        {
           
        }     

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "null")]
        [InlineData(null, "Bouvet")]
        [InlineData("", "Bouvet")]
        [InlineData("TestStudy", null)]
        [InlineData("TestStudy", "")]
        public async void ThrowOnMissingRequiredFields(string name, string vendor)
        {
            await ClearTestDatabase();

            var studyCreateService = StudyServiceMockFactory.CreateService(_serviceProvider);

            var initialStudy = new StudyCreateDto()
            {
                Name = "TestStudy12345",
                Vendor = "Equinor"
            };

            var createdStudy = await studyCreateService.CreateAsync(initialStudy);            

            var studyWithoutReqFields = new StudyUpdateDto()
            {              
                Name = name,
                Vendor = vendor
            };

            var studyUpdateHandler = StudyUpdateHandlerMockFactory.Create(_serviceProvider);
            await Assert.ThrowsAsync<ArgumentException>(() => studyUpdateHandler.UpdateAsync(createdStudy.Id, studyWithoutReqFields));
        }        
    }
}
