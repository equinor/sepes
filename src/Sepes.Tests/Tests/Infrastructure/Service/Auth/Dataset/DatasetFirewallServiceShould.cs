using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Mocks.ServiceMockFactory;
using Xunit;

namespace Sepes.Tests.Services.Infrastructure
{
    public class DatasetFirewallServiceShould : DatasetServiceTestBase
    {
        [Theory]
        [InlineData(2, "1.1.1.1", "1.1.1.2")]
        public async Task ShouldReturnTrue_AndAddIfNotExists(int expectedValue, string clientIp, string serverIp)
        {
            var datasetFirewallService = GetService(serverIp);

            var fireWallRules = new List<DatasetFirewallRule>(){ };
            fireWallRules.Add(new DatasetFirewallRule { Id = 1, Address = "1", Created = DateTime.Today.AddYears(-1) });
          
            var dataset = new Dataset() { FirewallRules = fireWallRules };
            var result = await datasetFirewallService.SetDatasetFirewallRules(dataset, clientIp);

            Assert.True(result);
            Assert.Equal(expectedValue, dataset.FirewallRules.Count);
        }

        [Theory]
        [InlineData("Client IP is not a valid IP Address", "abc", "dc11")]
        [InlineData("Server IP is not a valid IP Address", "1.1.1.1", "dc11")]
        [InlineData("Client IP is not a valid IP Address", "1a.1.a1.1", "1.1.1.1")]
        public async Task ThrowOnAddingInvalidIp(string expectedResult, string clientIp, string serverIp)
        {
            var datasetFirewallService = GetService(serverIp);        
    
            var dataset = new Dataset() { FirewallRules = new List<DatasetFirewallRule>() { new DatasetFirewallRule { Id = 1, Address = "1", Created = DateTime.Today.AddYears(-1) } } };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => datasetFirewallService.SetDatasetFirewallRules(dataset, clientIp));

            Assert.Equal(expectedResult, ex.Message);
        }

       
        IDatasetFirewallService GetService(string serverIp) { 
            return DatasetServiceMockFactory.GetStudyDatasetFirewallService(_serviceProvider, serverIp);
        }
    }
}
