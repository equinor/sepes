using Sepes.Common.Constants;
using Sepes.Common.Dto.Study;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Infrastructure.Util.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.DataModelService
{
    public class StudyListModelService : DapperModelWithPermissionServiceBase, IStudyListModelService
    {
        public StudyListModelService(IDapperQueryService dapperQueryService, IUserService userService, IStudyPermissionService studyPermissionService)
            : base(dapperQueryService, userService, studyPermissionService)
        {

        }

        public async Task<IEnumerable<StudyListItemDto>> GetListAsync()
        {
            IEnumerable<StudyListItemDto> studies;

            var user = await _userService.GetCurrentUserAsync();

            var studiesQuery = "SELECT DISTINCT [Id], [Name], [Description], [Vendor], [Restricted], [LogoUrl] FROM [dbo].[Studies] s";
            studiesQuery += " INNER JOIN [dbo].[StudyParticipants] sp on s.Id = sp.StudyId";
            studiesQuery += " WHERE s.Closed = 0";

            var studiesAccessWherePart = StudyAccessQueryBuilder.CreateAccessWhereClause(user, UserOperation.Study_Read);

            if (!string.IsNullOrWhiteSpace(studiesAccessWherePart))
            {
                studiesQuery += $" AND ({studiesAccessWherePart})";
            }

            studies = await RunDapperQueryMultiple<StudyListItemDto>(studiesQuery);
            return studies;
        }
    }
}
