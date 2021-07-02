using Dapper;
using Microsoft.Data.SqlClient;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Study;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class StudyDetailsModelService : DapperModelWithPermissionServiceBase, IStudyDetailsModelService
    {
        readonly string _dbConnectionString;

        public StudyDetailsModelService(IUserService userService, IDapperQueryService dapperQueryService, IDatabaseConnectionStringProvider databaseConnectionStringProvider, IStudyPermissionService studyPermissionService)
            : base(dapperQueryService, userService, studyPermissionService)
        {
            _dbConnectionString = databaseConnectionStringProvider.GetConnectionString();
        }

        public async Task<StudyDetailsDapper> GetStudyDetailsAsync(int studyId)
        {
            var studyQuery = "SELECT DISTINCT [Id] as StudyId, [Name], [Description], [WbsCode], [WbsCodeValid], [Vendor], [Restricted], [LogoUrl] FROM [dbo].[Studies] s WHERE Id=@studyId AND s.Closed = 0";

            var responseFromDbService = await RunSingleEntityQueryWithPermissionCheck<StudyDetailsDapper>(studyQuery, UserOperation.Study_Read, new { studyId });

            return responseFromDbService;
        }

        public async Task<IEnumerable<SandboxForStudyDetailsDapper>> GetSandboxForStudyDetailsAsync(int studyId)
        {
            var query = "SELECT DISTINCT [Id] as SandboxId, [Name] as SandboxName, [StudyId] FROM [dbo].[Sandboxes] s WHERE s.StudyId=@studyId AND s.Deleted = 0";

            return await RunDapperQueryMultiple<SandboxForStudyDetailsDapper>(query, new { studyId });
        }

        public async Task<IEnumerable<DatasetForStudyDetailsDapper>> GetDatasetsForStudyDetailsAsync(int studyId)
        {
            using (var connection = new SqlConnection(_dbConnectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var query = "SELECT DISTINCT ds.[Id] as DatasetId, ds.[Name] as DatasetName, (SELECT CASE WHEN ds.StudySpecific = 1 THEN sds.StudyId ELSE NULL END) as [StudyId],";
                query += " sb.[Id] as [SandboxId], sb.[Name] as [SandboxName]";
                query += " FROM [StudyDatasets] sds";
                query += " left join [dbo].[Datasets] ds on sds.DatasetId = ds.Id";
                query += " left join [dbo].[SandboxDatasets] sbds on ds.Id = sbds.DatasetId";
                query += " left join [dbo].[Sandboxes] sb on sbds.SandboxId = sb.Id and sb.[StudyId] = @studyId and sb.[Deleted] = 0";
                query += " WHERE sds.[StudyId] = @studyId and ds.[Deleted] = 0";

                var entityDictionary = new Dictionary<int, DatasetForStudyDetailsDapper>();

                var entities = await connection.QueryAsync<DatasetForStudyDetailsDapper, SandboxForStudyDetailsDapper, DatasetForStudyDetailsDapper>(query,
                     (dataset, sandbox) =>
                     {
                         if (!entityDictionary.TryGetValue(dataset.DatasetId, out DatasetForStudyDetailsDapper datasetEntry))
                         {
                             datasetEntry = dataset;

                             datasetEntry.Sandboxes = new List<SandboxForStudyDetailsDapper>();
                             entityDictionary.Add(dataset.DatasetId, datasetEntry);
                         }

                         if (sandbox != null && sandbox.SandboxId > 0)
                         {
                             datasetEntry.Sandboxes.Add(sandbox);
                         }


                         return datasetEntry;
                     },
                     param: new { studyId },
                    splitOn: "SandboxId"

                     );

                return entities.Distinct();
            }
        }

        public async Task<IEnumerable<StudyParticipantForStudyDetailsDapper>> GetParticipantsForStudyDetailsAsync(int studyId)
        {
            using (var connection = new SqlConnection(_dbConnectionString))
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var query = "SELECT u.[Id] as UserId, u.FullName, u.UserName, u.EmailAddress, sp.RoleName as [Role]";
                query += " FROM [dbo].[StudyParticipants] sp";
                query += " LEFT JOIN [dbo].[Users] u on sp.UserId = u.Id";
                query += " WHERE sp.[StudyId] = @studyId";

                return await RunDapperQueryMultiple<StudyParticipantForStudyDetailsDapper>(query, new { studyId });
            }
        }

        public async Task<StudyResultsAndLearningsDto> GetResultsAndLearningsAsync(int studyId)
        {
            var resultsAndLearningsQuery = "SELECT DISTINCT [Id] as [StudyId], [ResultsAndLearnings] FROM [dbo].[Studies] s WHERE Id=@studyId AND s.Closed = 0";

            var responseFromDbService = await RunSingleEntityQueryWithPermissionCheck<StudyResultsAndLearnings>(resultsAndLearningsQuery, UserOperation.Study_Read_ResultsAndLearnings, new { studyId });

            return new StudyResultsAndLearningsDto() { ResultsAndLearnings = responseFromDbService.ResultsAndLearnings };
        }


    }
}
