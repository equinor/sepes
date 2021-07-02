using Sepes.Common.Constants;
using Sepes.Common.Dto.Study;
using Sepes.Common.Exceptions;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using Xunit;

namespace Sepes.Tests.Services.DomainServices
{
    public class StudyDeleteServiceTests : TestBaseWithInMemoryDb
    {
        public StudyDeleteServiceTests()
            :base()
        {
          
        }        
        

        [Theory]
        [InlineData(2)]
        [InlineData(7)]
        [InlineData(99999)]
        [InlineData(123456)]
        public async void DeleteStudyAsync_ShouldThrow_IfStudyDoesNotExist(int id)
        {
            await ClearTestDatabase();
            var studyDeleteService = StudyServiceMockFactory.DeleteService(_serviceProvider);

            await Assert.ThrowsAsync<NotFoundException>(() => studyDeleteService.DeleteStudyAsync(id));
        }

        [Fact]
        public async void CloseStudyAsync_ShouldClose_IfStudyExists()
        {
            await ClearTestDatabase();
            var studyDeleteService = StudyServiceMockFactory.DeleteService(_serviceProvider);

            var initialStudy = new StudyCreateDto()
            {
                Name = "TestStudy12345",
                Vendor = "Equinor"
            };

            var studyCreateService = StudyServiceMockFactory.CreateService(_serviceProvider);
            var createdStudy = await studyCreateService.CreateAsync(initialStudy);
           
            await studyDeleteService.CloseStudyAsync(createdStudy.Id);
         
            var studyReadService = StudyModelServiceMockFactory.StudyEfModelService(_serviceProvider);
            _ = await Assert.ThrowsAsync<NotFoundException>(() => studyReadService.GetByIdAsync(createdStudy.Id, UserOperation.Study_Read));

        }  
    }
}
