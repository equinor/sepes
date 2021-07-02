using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class DapperModelWithPermissionServiceBase
    {
        protected readonly IUserService _userService;
        protected readonly IDapperQueryService _dapperQueryService;
        readonly IStudyPermissionService _studyPermissionService;

        public DapperModelWithPermissionServiceBase(IDapperQueryService dapperQueryService, IUserService userService, IStudyPermissionService studyPermissionService)
          
        {
            _userService = userService;
            _dapperQueryService = dapperQueryService;
            _studyPermissionService = studyPermissionService;
        }

        protected async Task<T> RunSingleEntityQueryWithPermissionCheck<T>(string dataQuery, UserOperation operation, object parameters = null) where T : SingleEntityDapperResult
        {
            var currentUser = await _userService.GetCurrentUserAsync();
           return await RunSingleEntityQueryWithPermissionCheck<T>(currentUser, dataQuery, operation, parameters);
        }

        protected async Task<T> RunSingleEntityQueryWithPermissionCheck<T>(UserDto currentUser, string dataQuery, UserOperation operation, object parameters = null) where T : SingleEntityDapperResult
        {
            var completeQuery = WrapSingleEntityQueryWithAccessProjection(currentUser, dataQuery, operation);
            var singleEntity = await RunDapperQuerySingleAsync<T>(completeQuery, parameters);

            _studyPermissionService.VerifyAccessOrThrow(singleEntity, currentUser, operation);

            return singleEntity;
        }

        protected string WrapSingleEntityQueryWithAccessProjection(UserDto currentUser, string dataQuery, UserOperation operation)
        {
            var accessWherePart = StudyAccessQueryBuilder.CreateAccessWhereClause(currentUser, operation);

            var completeQuery = $"WITH dataCte AS ({dataQuery})";
            completeQuery += " ,accessCte as (SELECT [Id] FROM Studies s INNER JOIN [dbo].[StudyParticipants] sp on s.Id = sp.StudyId WHERE s.Id=@studyId";

            if (!string.IsNullOrWhiteSpace(accessWherePart))
            {
                completeQuery += $" AND ({accessWherePart})";
            }

            completeQuery += " ) SELECT DISTINCT d.*, (CASE WHEN a.Id IS NOT NULL THEN 1 ELSE 0 END) As Authorized from dataCte d LEFT JOIN accessCte a on d.StudyId = a.Id ";

            return completeQuery;
        }


        protected async Task ExecuteAsync(string statement, object parameters = null)
        {
            await _dapperQueryService.ExecuteAsync(statement, parameters);
        }

        protected async Task<IEnumerable<T>> RunDapperQueryMultiple<T>(string query, object parameters = null)
        {
            return await _dapperQueryService.RunDapperQueryMultiple<T>(query, parameters);
        }

        protected async Task<T> RunDapperQuerySingleAsync<T>(string query, object parameters = null) 
        {
            return await _dapperQueryService.RunDapperQuerySingleAsync<T>(query, parameters);
        }
    }
}

