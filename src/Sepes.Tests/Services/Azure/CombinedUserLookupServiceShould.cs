﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sepes.Azure.Dto;
using Sepes.Azure.Service;
using Sepes.Azure.Service.Interface;
using Sepes.Tests.Common.Constants;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sepes.Tests.Services.Azure
{
    public class CombinedUserLookupServiceShould : ServiceTestBase
    {
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ReturnEmptyListIfBackingSytemFailsError(bool companyLookupThrows, bool affiliateLookupThrows)
        {
            var service = GetServiceWithMocks(companyLookupThrows, affiliateLookupThrows, TestUserConstants.SOME_EMPLOYEE, TestUserConstants.SOME_EMPLOYEE);

            var result = await service.SearchAsync("someuser", 10);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]      
        public async Task ReturnItemsFromOneServiceIfOtherServiceFails(bool companyLookupThrows, bool affiliateLookupThrows)
        {
            var service = GetServiceWithMocks(companyLookupThrows, affiliateLookupThrows, TestUserConstants.SOME_EMPLOYEE, TestUserConstants.SOME_EMPLOYEE);

            var result = await service.SearchAsync("someuser", 10);

            Assert.NotEmpty(result);
            Assert.Single(result);
        }

        [Fact]       
        public async Task ReturnUniqueItemsIfMultipleSearchHits()
        {
            var service = GetServiceWithMocks(false, false, TestUserConstants.SOME_EMPLOYEE, TestUserConstants.SOME_EMPLOYEE);

            var result = await service.SearchAsync("someuser", 10);

            Assert.NotEmpty(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task MergeResultsFromAllSources()
        {
            var service = GetServiceWithMocks(false, false, TestUserConstants.SOME_EMPLOYEE, TestUserConstants.SOME_AFFILIATE);

            var result = await service.SearchAsync("someuser", 10);

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);

            var idx = 0;

            foreach(var curKeys in result.Keys)
            {
                var currentUser = result[curKeys];

                if (idx == 0)
                {
                    Assert.Equal(TestUserConstants.SOME_EMPLOYEE.Id, currentUser.Id);
                 
                }
                else if (idx == 1)
                {
                    Assert.Equal(TestUserConstants.SOME_AFFILIATE.Id, currentUser.Id);
                }               

                idx++;
            }
        }

        CombinedUserLookupService GetServiceWithMocks(bool companyLookupThrows, bool affiliateLookupThrows, AzureUserDto companyUserResult, AzureUserDto affiliateUserResult)
        {
            var logger = _serviceProvider.GetService<ILogger<CombinedUserLookupService>>();
            var config = _serviceProvider.GetService<IConfiguration>();
            var companyLookupService = GetAzureLookupService(companyLookupThrows, companyUserResult);
            var affiliateLookupService = GetAzureLookupService(affiliateLookupThrows, affiliateUserResult);

            return new CombinedUserLookupService(logger, config, companyLookupService.Object, affiliateLookupService.Object);
        }

        Mock<IUserFromGroupLookupService> GetAzureLookupService(bool failing, AzureUserDto user)
        {
            if (failing)
            {
                return GetFailingAzureLookupService();
            }
            else
            {
                var users = new Dictionary<string, AzureUserDto>();

                if (user != null)
                {
                    users.Add(user.Id, user);
                }

                return GetSucceedingAzureLookupService(users);
            }
        }

        Mock<IUserFromGroupLookupService> GetFailingAzureLookupService()
        {
            var mock = new Mock<IUserFromGroupLookupService>();
            mock.Setup(s => s.SearchInGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Some exception"));
            return mock;
        }

        Mock<IUserFromGroupLookupService> GetSucceedingAzureLookupService(Dictionary<string, AzureUserDto> users)
        {
            var mock = new Mock<IUserFromGroupLookupService>();
            mock.Setup(s => s.SearchInGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(users);
            return mock;
        }
    }
}
