using Sepes.Common.Constants;
using Sepes.Common.Exceptions;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Sepes.Tests.Setup;
using Sepes.Tests.Tests;
using Xunit;

namespace Sepes.Tests.Services.DomainServices
{
    public class StudyReadServiceTests : TestBaseWithInMemoryDb
    {

        [Theory]
        [InlineData(2)]
        [InlineData(7)]
        [InlineData(99999)]
        [InlineData(123456)]
        public async void GetStudyByIdAsync_WillThrow_IfStudyDoesNotExist(int id)
        {
            await ClearTestDatabase();
            var studyService = StudyModelServiceMockFactory.StudyEfModelService(_serviceProvider);

            await Assert.ThrowsAsync<NotFoundException>(() => studyService.GetByIdAsync(id, UserOperation.Study_Read));
        }       
    }
}
