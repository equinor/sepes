﻿using Sepes.RestApi.IntegrationTests.Dto;
using System.Net;
using Xunit;

namespace Sepes.RestApi.IntegrationTests.TestHelpers.AssertSets
{
    public static class ApiResponseBasicAsserts
    {
        public static void ExpectSuccess(ApiResponseWrapper responseWrapper)
        {
            Assert.Equal(HttpStatusCode.OK, responseWrapper.StatusCode);
        }

        public static void ExpectNoContent(ApiResponseWrapper responseWrapper)
        {
            Assert.Equal(HttpStatusCode.NoContent, responseWrapper.StatusCode);
        }

        public static void ExpectSuccess<T>(ApiResponseWrapper<T> responseWrapper)
        {        
            Assert.Equal(HttpStatusCode.OK, responseWrapper.StatusCode);
            Assert.NotNull(responseWrapper.Content);          
        }

        public static void ExpectFailure(ApiResponseWrapper responseWrapper, HttpStatusCode expectedStatusCode)
        {
            Assert.Equal(expectedStatusCode, responseWrapper.StatusCode);
        }

        public static void ExpectFailureWithMessage(ApiResponseWrapper<Infrastructure.Dto.ErrorResponse> responseWrapper, HttpStatusCode expectedStatusCode, string messageShouldContain = null)
        {
            Assert.Equal(expectedStatusCode, responseWrapper.StatusCode);            

            if (!string.IsNullOrWhiteSpace(messageShouldContain))
            {
                Assert.Contains(messageShouldContain, responseWrapper.Content.Message);
            }
        }

        public static void ExpectForbidden(ApiResponseWrapper responseWrapper)
        {
            ExpectFailure(responseWrapper, HttpStatusCode.Forbidden);

        }

        public static void ExpectForbiddenWithMessage(ApiResponseWrapper<Infrastructure.Dto.ErrorResponse> responseWrapper, string messageShouldContain = null)
        {
            ExpectFailureWithMessage(responseWrapper, HttpStatusCode.Forbidden, messageShouldContain);
        }      
    }
}
