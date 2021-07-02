using Sepes.Common.Constants;
using Sepes.Infrastructure.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService.Interface
{
    public interface IStudyEfModelOperationsService
    {
        Task<Study> GetStudyFromQueryableThrowIfNotFound(IQueryable<Study> queryable, int studyId);
        Task<Study> GetStudyFromQueryableThrowIfNotFoundOrNoAccess(IQueryable<Study> queryable, int studyId, UserOperation operation, string roleBeingAddedOrRemoved = null);
    }
}
