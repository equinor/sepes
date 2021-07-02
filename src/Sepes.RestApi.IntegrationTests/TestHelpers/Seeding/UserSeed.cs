using Sepes.RestApi.IntegrationTests.TestHelpers;
using Sepes.Tests.Common.Constants;
using System;
using System.Threading.Tasks;

namespace Sepes.RestApi.IntegrationTests.Setup.Seeding
{
    public static class UserSeed
    {
        public static async Task Seed()
        {            
                var testUser = new Infrastructure.Model.User()
                {                  
                    EmailAddress = UserTestConstants.COMMON_CUR_USER_EMAIL,
                    ObjectId = UserTestConstants.COMMON_CUR_USER_OBJECTID,
                    FullName = UserTestConstants.COMMON_CUR_USER_FULL_NAME,
                    UserName = UserTestConstants.COMMON_CUR_USER_UPN,
                    CreatedBy = "seed",
                    UpdatedBy = "seed",
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                };

                await SliceFixture.InsertAsync(testUser);

                var someOtherParticipant = new Infrastructure.Model.User()
                {                   
                    EmailAddress = UserTestConstants.COMMON_NEW_PARTICIPANT_EMAIL,
                    ObjectId = UserTestConstants.COMMON_NEW_PARTICIPANT_OBJECTID,
                    FullName = UserTestConstants.COMMON_NEW_PARTICIPANT_FULL_NAME,
                    UserName = UserTestConstants.COMMON_NEW_PARTICIPANT_UPN,
                    CreatedBy = "seed",
                    UpdatedBy = "seed",
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,

                };

                await SliceFixture.InsertAsync(someOtherParticipant);

                var someOtherStudyOwner = new Infrastructure.Model.User()
                {                  
                    EmailAddress = UserTestConstants.COMMON_ALTERNATIVE_STUDY_OWNER_EMAIL,
                    ObjectId = UserTestConstants.COMMON_ALTERNATIVE_STUDY_OWNER_OBJECTID,
                    FullName = UserTestConstants.COMMON_ALTERNATIVE_STUDY_OWNER_FULL_NAME,
                    UserName = UserTestConstants.COMMON_ALTERNATIVE_STUDY_OWNER_UPN,
                    CreatedBy = "seed",
                    UpdatedBy = "seed",
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                };

                await SliceFixture.InsertAsync(someOtherStudyOwner);          
        }
    }
}
