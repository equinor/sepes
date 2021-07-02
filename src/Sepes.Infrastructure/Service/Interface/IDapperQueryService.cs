using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.Interface
{
    public interface IDapperQueryService
    {
        Task ExecuteAsync(string statement, object parameters = null);
        Task<IEnumerable<T>> RunDapperQueryMultiple<T>(string query, object parameters = null);
        Task<T> RunDapperQuerySingleAsync<T>(string query, object parameters = null);
    }
}
