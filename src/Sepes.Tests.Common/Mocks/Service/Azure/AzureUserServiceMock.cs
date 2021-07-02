using Microsoft.Graph;
using Sepes.Azure.Dto;
using Sepes.Azure.Service.Interface;
using Sepes.Tests.Common.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.RestApi.IntegrationTests.Setup
{
    public class AzureUserServiceMock : IAzureUserService
    {
        public Task<AzureUserDto> GetUserAsync(string id)
        {
            var user = new AzureUserDto { UserPrincipalName = UserTestConstants.COMMON_CUR_USER_UPN, DisplayName = UserTestConstants.COMMON_CUR_USER_FULL_NAME, Mail = UserTestConstants.COMMON_CUR_USER_EMAIL };
            return Task.FromResult(user);
        }      

        public Task<List<User>> SearchUsersAsync(string search, int limit, CancellationToken cancellationToken = default)
        {
            var userList = new List<User>
            {
                new User { Id = UserTestConstants.COMMON_NEW_PARTICIPANT_OBJECTID, DisplayName = UserTestConstants.COMMON_NEW_PARTICIPANT_FULL_NAME, Mail = UserTestConstants.COMMON_NEW_PARTICIPANT_EMAIL, UserPrincipalName = UserTestConstants.COMMON_NEW_PARTICIPANT_UPN }
            };

            return Task.FromResult(userList);
        }       
    }
}
