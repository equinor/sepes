using System;
using System.Threading;
using System.Threading.Tasks;
using Sepes.Infrastructure.Service.Interface;

namespace Sepes.Tests.Common.Mocks.Service
{
    public class PublicIpFromThirdPartyServiceMock : IPublicIpFromThirdPartyService
    {
        int tryCounter = 0;

        readonly int _succeedAfter;
        readonly string _ipToReturn;

        public PublicIpFromThirdPartyServiceMock(int succeedAfter, string ipToReturn)
        {
            _succeedAfter = succeedAfter;
            _ipToReturn = ipToReturn;
        }

        public async Task<string> GetIp(string curIpUrl, CancellationToken cancellation = default)
        {
            tryCounter++;

            if (tryCounter == _succeedAfter)
            {
                return await Task.FromResult(_ipToReturn);
            }                    

            throw new Exception("Planned failure from moq");
        }
    
    }  
}
