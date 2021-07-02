using Microsoft.AspNetCore.Http;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Handlers.Interface;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Service.Queries;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Handlers
{
    public class StudyCreateLogoHandler : IStudyCreateLogoHandler
    {
        readonly SepesDbContext _db;      
        readonly IStudyLogoCreateService _studyLogoCreateService;
        readonly IStudyEfModelOperationsService _studyEfModelOperationsService;

        public StudyCreateLogoHandler(SepesDbContext db, IStudyLogoCreateService studyLogoCreateService, IStudyEfModelOperationsService studyEfModelOperationsService)
        {
            _db = db;               
            _studyLogoCreateService = studyLogoCreateService;
            _studyEfModelOperationsService = studyEfModelOperationsService;
        }

        public async Task<string> CreateAsync(int studyId, IFormFile studyLogo)
        {
            var study = await GetStudyAsync(studyId);
            var logoUrl = await _studyLogoCreateService.CreateAsync(study, studyLogo);
            return logoUrl;
        }        

        public async Task<Study> GetStudyAsync(int studyId)
        {
            var queryable = StudyBaseQueries.ActiveStudiesWithParticipantsQueryable(_db);
            return await _studyEfModelOperationsService.GetStudyFromQueryableThrowIfNotFoundOrNoAccess(queryable, studyId, UserOperation.Study_Update_Metadata);
        }      
    }
}
