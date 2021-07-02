using Microsoft.EntityFrameworkCore;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public class DatabaseConnectionStringProviderMock : IDatabaseConnectionStringProvider
    {
        readonly SepesDbContext _db;

        public DatabaseConnectionStringProviderMock (SepesDbContext db)
        {
            _db = db;          
        }

        public string GetConnectionString()
        {
            return _db.Database.GetDbConnection().ConnectionString;
        }
    }
}
