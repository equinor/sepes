﻿using Sepes.Infrastructure.Dto.Study;
using Sepes.RestApi.IntegrationTests.Dto;
using Sepes.RestApi.IntegrationTests.TestHelpers;
using System.Threading.Tasks;

namespace Sepes.RestApi.IntegrationTests.RequestHelpers
{
    public static class StudyCreator
    {
        static async Task<ApiConversation<StudyCreateDto, TResponse>> Create<TResponse>(RestHelper restHelper, string studyName = "studyName", string vendor = "Vendor", string wbsCode = "wbs")         
        {
            var request = new StudyCreateDto() { Name = studyName, Vendor = vendor, WbsCode = wbsCode };
            var response = await restHelper.Post<TResponse, StudyCreateDto>("api/studies", request);

            return new ApiConversation<StudyCreateDto, TResponse>(request, response);
        }

        public static async Task<ApiConversation<StudyCreateDto, StudyDto>> CreateAndExpectSuccess(RestHelper restHelper, string studyName = "studyName", string vendor = "Vendor", string wbsCode = "wbs")
        {
            return await Create<StudyDto>(restHelper, studyName, vendor, wbsCode);
        }

        public static async Task<ApiConversation<StudyCreateDto, Infrastructure.Dto.ErrorResponse>> CreateAndExpectFailure(RestHelper restHelper, string studyName = "studyName", string vendor = "Vendor", string wbsCode = "wbs")
        {
            return await Create<Infrastructure.Dto.ErrorResponse>(restHelper, studyName, vendor, wbsCode);
        }
    } 
}