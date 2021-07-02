using System.Collections.Generic;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;

namespace Sepes.Tests.ModelFactory
{
    public static class StudyPopulator
    {
        public static void Add(SepesDbContext db, string studyName, string vendor, string wbs, int ownerId)
        {
            var newStudy = StudyFactory.Create(studyName, vendor, wbs, new List<StudyParticipant>() { new StudyParticipant() { UserId = ownerId, RoleName = StudyRoles.StudyOwner } });


            db.Studies.Add(newStudy);
        }
    }
}
