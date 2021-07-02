using Sepes.Azure.Dto;

namespace Sepes.Tests.Common.Constants
{
    public static class UserTestConstants
    {
        //CURRENT USER CONSTANTS
        public const int COMMON_CUR_USER_DB_ID = 1;
        public const string COMMON_CUR_USER_OBJECTID = "21dcfa89-2da8-4bdc-80bd-b2c7436190c9";
        public const string COMMON_CUR_USER_EMAIL = "testuser@equinor.com";
        public const string COMMON_CUR_USER_UPN = "testuser@equinor.com";
        public const string COMMON_CUR_USER_FULL_NAME = "Unit Test Current User";

        //ADD PARTICIPANT USER CONSTANTS
        public const int COMMON_NEW_PARTICIPANT_DB_ID = 2;
        public const string COMMON_NEW_PARTICIPANT_OBJECTID = "c4c3e5a0-5a73-484a-b5be-e3903b4d2788";
        public const string COMMON_NEW_PARTICIPANT_EMAIL = "newuser@equinor.com";
        public const string COMMON_NEW_PARTICIPANT_UPN = "newuser@equinor.com";
        public const string COMMON_NEW_PARTICIPANT_FULL_NAME = "Unit Test New User";

        //ADD PARTICIPANT USER CONSTANTS
        public const int COMMON_ALTERNATIVE_STUDY_OWNER_DB_ID = 3;
        public const string COMMON_ALTERNATIVE_STUDY_OWNER_OBJECTID = "1164962e-cad1-40b5-a246-84b36dce0dcd";
        public const string COMMON_ALTERNATIVE_STUDY_OWNER_EMAIL = "anotherstudyowner@equinor.com";
        public const string COMMON_ALTERNATIVE_STUDY_OWNER_UPN = "anotherstudyowner@equinor.com";
        public const string COMMON_ALTERNATIVE_STUDY_OWNER_FULL_NAME = "Anther Study Owner";

        //SOME RANDOM AZURE USER       
        public const string COMMON_AZURE_USER_OBJECTID = "d862f643-e318-4807-b682-57f4663bafc3";
        public const string COMMON_AZURE_USER_EMAIL = "someazureuser@equinor.com";
        public const string COMMON_AZURE_USER_UPN = "someazureuser@equinor.com";
        public const string COMMON_AZURE_USER_FULL_NAME = "Some Azure User";

        //SOME AFFILIATE AZURE USER       
        public const string COMMON_AFFILIATE_OBJECTID = "9ed8b795-a14e-48c3-98f1-10b3bc85c62f";
        public const string COMMON_AFFILIATE_EMAIL = "someaffiliate@equinor.com";
        public const string COMMON_AFFILIATE_UPN = "someaffiliate@equinor.com";
        public const string COMMON_AFFILIATE_FULL_NAME = "Some Affiliate User";

        public static AzureUserDto NEW_PARTICIPANT = new AzureUserDto
        {
            Id = COMMON_NEW_PARTICIPANT_OBJECTID,
            DisplayName = COMMON_NEW_PARTICIPANT_FULL_NAME,
            Mail = COMMON_NEW_PARTICIPANT_EMAIL,
            UserPrincipalName = COMMON_NEW_PARTICIPANT_UPN
        };

        public static AzureUserDto SOME_EMPLOYEE = new AzureUserDto
        {
            Id = COMMON_AZURE_USER_OBJECTID,
            DisplayName = COMMON_AZURE_USER_FULL_NAME,
            Mail = COMMON_AZURE_USER_EMAIL,
            UserPrincipalName = COMMON_AZURE_USER_UPN
        };

        public static AzureUserDto SOME_AFFILIATE = new AzureUserDto
        {
            Id = COMMON_AFFILIATE_OBJECTID,
            DisplayName = COMMON_AFFILIATE_FULL_NAME,
            Mail = COMMON_AFFILIATE_EMAIL,
            UserPrincipalName = COMMON_AFFILIATE_UPN
        };
    }
}
