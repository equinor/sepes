using System;
using Sepes.Infrastructure.Model;
using Sepes.Tests.Common.Constants;

namespace Sepes.Tests.Common.ModelFactory
{
    public static class StudyModelFactory
    {
        public static Study CreateBasic(int id = StudyTestConstants.CREATED_BY_ME_ID, string name = StudyTestConstants.CREATED_BY_ME_NAME, string vendor = StudyTestConstants.CREATED_BY_ME_VENDOR, string wbs = StudyTestConstants.CREATED_BY_ME_WBS, bool wbsValid = false, DateTime? wbsCodeValidated = default(DateTime?) ) {
            return new Study() {
                Id = id,
                Name = name,
                Vendor = vendor,
                WbsCode = wbs,
                WbsCodeValid = wbsValid,
                WbsCodeValidatedAt = wbsCodeValidated
            };
        } }
}
