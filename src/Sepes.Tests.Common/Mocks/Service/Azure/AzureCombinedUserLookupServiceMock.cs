using Sepes.Azure.Dto;
using Sepes.Azure.Service.Interface;
using Sepes.Tests.Common.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.RestApi.IntegrationTests.Setup
{
    public class AzureCombinedUserLookupServiceMock : ICombinedUserLookupService
    {
        public Task<Dictionary<string, AzureUserDto>> SearchAsync(string search, int limit, CancellationToken cancellationToken = default)
        {
            var userList = new Dictionary<string, AzureUserDto>
            {
                {
                UserTestConstants.COMMON_NEW_PARTICIPANT_OBJECTID,
                 UserTestConstants.NEW_PARTICIPANT
                }
            };

            return Task.FromResult(userList);
        }
    }
}
