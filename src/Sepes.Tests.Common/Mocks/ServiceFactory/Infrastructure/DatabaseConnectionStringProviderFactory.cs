using Microsoft.EntityFrameworkCore;
using Moq;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class DatabaseConnectionStringProviderFactory
    {
        public static IDatabaseConnectionStringProvider Create(SepesDbContext db)
        {
            var mockedService = new Mock<IDatabaseConnectionStringProvider>();
            mockedService.Setup(s => s.GetConnectionString()).Returns(db.Database.GetDbConnection().ConnectionString);
            return mockedService.Object;
        }      
    }
}
