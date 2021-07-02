using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Tests.Tests;
using System.Threading.Tasks;

namespace Sepes.Tests.Services.DomainServices
{
    public class StudyParticipantSearchTestBase : TestBaseWithInMemoryDb
    {
        protected async Task<SepesDbContext> RefreshAndSeedTestDatabase()
        {
            var db = await ClearTestDatabase();

            var user = new User()
            {
                Id = 1,
                FullName = "John Doe",
                EmailAddress = "John@hotmail.com",
                ObjectId = "1"
            };

            var user2 = new User()
            {
                Id = 2,
                FullName = "John Doe2",
                EmailAddress = "John2@hotmail.com",
                ObjectId = "2"
            };

            db.Users.Add(user);
            db.Users.Add(user2);
            await db.SaveChangesAsync();
            return db;
        }
    }
}
