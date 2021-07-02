using Microsoft.EntityFrameworkCore;
using Sepes.Common.Constants;
using Sepes.Common.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class StudyEfModelOperationsService : IStudyEfModelOperationsService
    {      
        readonly IStudyPermissionService _studyPermissionService;

        public StudyEfModelOperationsService(IStudyPermissionService studyPermissionService)       
        {           
            _studyPermissionService = studyPermissionService;
        }         

        public async Task<Study> GetStudyFromQueryableThrowIfNotFoundOrNoAccess(IQueryable<Study> queryable, int studyId, UserOperation operation, string roleBeingAddedOrRemoved = null)
        {
            var study = await GetStudyFromQueryableThrowIfNotFound(queryable, studyId);

            await _studyPermissionService.VerifyAccessOrThrow(study, operation, roleBeingAddedOrRemoved);

            return study;
        }

        public async Task<Study> GetStudyFromQueryableThrowIfNotFound(IQueryable<Study> queryable, int studyId)
        {
            var study = await queryable.SingleOrDefaultAsync(s => s.Id == studyId);
           
            if (study == null)
            {
                throw NotFoundException.CreateForEntity("Study", studyId);
            }

            return study;
        }     
    }
}
