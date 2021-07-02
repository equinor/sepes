using Sepes.Common.Constants;
using Sepes.Common.Dto.Study;
using Sepes.Infrastructure.Handlers.Interface;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Service.Queries;
using System;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers
{
    public class StudyResultsAndLearningsUpdateHandler : IStudyResultsAndLearningsUpdateHandler
    {
        readonly SepesDbContext _db;
        readonly IUserService _userService;
        readonly IStudyEfModelOperationsService _studyEfModelOperationsService;

        public StudyResultsAndLearningsUpdateHandler(SepesDbContext db, IUserService userService, IStudyEfModelOperationsService studyEfModelOperationsService)
        {
            _userService = userService;
            _db = db;
            _studyEfModelOperationsService = studyEfModelOperationsService;
        }

        public async Task<StudyResultsAndLearningsDto> UpdateAsync(int studyId, StudyResultsAndLearningsDto resultsAndLearnings)
        {
            var studyFromDb = await GetStudyAsync(studyId);

            if (resultsAndLearnings.ResultsAndLearnings != studyFromDb.ResultsAndLearnings)
            {
                studyFromDb.ResultsAndLearnings = resultsAndLearnings.ResultsAndLearnings;
            }

            var currentUser = await _userService.GetCurrentUserAsync();
            studyFromDb.Updated = DateTime.UtcNow;
            studyFromDb.UpdatedBy = currentUser.UserName;

            await _db.SaveChangesAsync();

            return new StudyResultsAndLearningsDto() { ResultsAndLearnings = studyFromDb.ResultsAndLearnings };
        }

        public async Task<Study> GetStudyAsync(int studyId)
        {
            var queryable = StudyBaseQueries.ActiveStudiesWithParticipantsQueryable(_db);
            return await _studyEfModelOperationsService.GetStudyFromQueryableThrowIfNotFoundOrNoAccess(queryable, studyId, UserOperation.Study_Update_Metadata);
        }
    }
}
