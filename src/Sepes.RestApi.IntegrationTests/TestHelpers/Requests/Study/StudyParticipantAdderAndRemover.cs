using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Common.Dto.Study;
using Sepes.RestApi.IntegrationTests.TestHelpers.Dto;
using Sepes.Tests.Common.Constants;
using System.Threading.Tasks;

namespace Sepes.RestApi.IntegrationTests.TestHelpers.Requests
{
    public static class StudyParticipantAdderAndRemover
    {
        static async Task<ApiConversation<ParticipantLookupDto, TResponse>> Add<TResponse>(RestHelper restHelper, int studyId, string role, ParticipantLookupDto requestDto)         
        {         
            var response = await restHelper.Put<TResponse, ParticipantLookupDto>($"api/studies/{studyId}/participants/{role}", requestDto);

            return new ApiConversation<ParticipantLookupDto, TResponse>(requestDto, response);
        }

        public static async Task<ApiConversation<ParticipantLookupDto, StudyParticipantListItem>> AddAndExpectSuccess(RestHelper restHelper, int studyId, string role, ParticipantLookupDto requestDto)
        {
            return await Add<StudyParticipantListItem>(restHelper, studyId, role, requestDto);
        }

        public static async Task<ApiConversation<ParticipantLookupDto, ErrorResponse>> AddAndExpectFailure(RestHelper restHelper, int studyId, string role, ParticipantLookupDto requestDto)
        {
            return await Add<ErrorResponse>(restHelper, studyId, role, requestDto);
        }

        static async Task<ApiConversation<TResponse>> Remove<TResponse>(RestHelper restHelper, int studyId, int userId, string role)
        {            
            var response = await restHelper.Delete<TResponse>($"api/studies/{studyId}/participants/{userId}/{role}");
            return new ApiConversation<TResponse>(response);
        }

        public static async Task<ApiConversation<StudyParticipantDto>> RemoveAndExpectSuccess(RestHelper restHelper, int studyId, int userId, string role)
        {
            return await Remove<StudyParticipantDto>(restHelper, studyId, userId, role);
        }

        public static async Task<ApiConversation<ErrorResponse>> RemoveAndExpectFailure(RestHelper restHelper, int studyId, int userId, string role)
        {
            return await Remove<ErrorResponse>(restHelper, studyId, userId, role);
        }

        public static ParticipantLookupDto CreateParticipantLookupDto()
        {
            return new ParticipantLookupDto()
            {
                Source = ParticipantSource.Db,
                DatabaseId = UserTestConstants.COMMON_NEW_PARTICIPANT_DB_ID,
                EmailAddress = UserTestConstants.COMMON_NEW_PARTICIPANT_EMAIL,
                FullName = UserTestConstants.COMMON_NEW_PARTICIPANT_FULL_NAME,
                ObjectId = UserTestConstants.COMMON_NEW_PARTICIPANT_OBJECTID,
                UserName = UserTestConstants.COMMON_NEW_PARTICIPANT_UPN
            };
        }
    } 
}
