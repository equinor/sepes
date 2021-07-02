using System;
using System.Collections.Generic;
using Sepes.Infrastructure.Model;

namespace Sepes.Tests.ModelFactory
{
    public static class StudyFactory
    {

        public static Study Create(string name = "studyName", string vendor = "defaultVendor", string wbs = "defaultWbs", params string[] participantsSeparated)
        {
            var participants = CreateParticipantList(participantsSeparated);

            return Create(name, vendor, wbs, participants);


        }

        public static Study Create(string name = "studyName", string vendor = "defaultVendor", string wbs = "defaultWbs", List<StudyParticipant> participants = null)
        {
            var newStudy = new Study()
            {
                Name = name,
                Vendor = vendor,
                WbsCode = wbs,
                Created = DateTime.UtcNow,
                CreatedBy = "unittest",
                Description = "Description for " + name,
                StudyParticipants = participants != null ? participants : new List<StudyParticipant>()
            };

            return newStudy;
        }

        public static List<StudyParticipant> CreateParticipantList(params string[] participantsSeparated)
        {
            var participants = new List<StudyParticipant>();

            foreach (var curParticipant in participantsSeparated)
            {
                var partsSeparated = curParticipant.Split(";");

                participants.Add(new StudyParticipant() { UserId = Convert.ToInt32(partsSeparated[0]), RoleName = partsSeparated[1] });
            }

            return participants;
        }
    }
}
