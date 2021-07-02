using Moq;
using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Infrastructure.Service.Interface;
using Sepes.Tests.Common.Constants;

namespace Sepes.Tests.Mocks.ServiceMockFactory
{
    public static class UserServiceMockFactory
    {
        public static string OBJECT_ID = UserTestConstants.COMMON_CUR_USER_OBJECTID;
        public static string USERNAME = UserTestConstants.COMMON_CUR_USER_UPN;
        public static string FULLNAME = UserTestConstants.COMMON_CUR_USER_FULL_NAME;
        public static string EMAIL = UserTestConstants.COMMON_CUR_USER_EMAIL;

        public static UserDto GetBasicAuthenticatedUser(bool employee, int userId)
        {
            var testUser = new UserDto(OBJECT_ID, USERNAME, FULLNAME, EMAIL);
            SetBasicUserProperties(testUser, employee, userId);
            return testUser;
        }

        //public static UserDto GetUserWithStudyParticipants(List<StudyParticipantDto>)
        //{
        //    var testUser = new UserDto(OBJECT_ID, USERNAME, FULLNAME, EMAIL);
        //    testUser.StudyParticipants.AddRange();
        //    return testUser;
        //}

        public static UserDto GetAdmin(int userId, string objectId = UserTestConstants.COMMON_CUR_USER_OBJECTID) {

            var testUser = new UserDto(objectId, USERNAME, FULLNAME, EMAIL, admin: true);
            SetBasicUserProperties(testUser, true, userId);
            return testUser;
        }

        public static UserDto GetSponsor(int userId)
        {
            var testUser = new UserDto(OBJECT_ID, USERNAME, FULLNAME, EMAIL, sponsor: true);
            SetBasicUserProperties(testUser, true, userId);
            return testUser;
        }

        public static UserDto GetDatasetAdmin(int userId)
        {
            var testUser = new UserDto(OBJECT_ID, USERNAME, FULLNAME, EMAIL, datasetAdmin: true);
            SetBasicUserProperties(testUser, true, userId);
            return testUser;
        }

        public static void SetBasicUserProperties(UserDto user, bool employee, int userId)
        {
            user.Id = userId;
            user.Employee = employee;

            if (user.Admin)
            {
                user.AppRoles.Add(AppRoles.Admin);
            }

            if (user.Sponsor)
            {
                user.AppRoles.Add(AppRoles.Sponsor);
            }

            if (user.DatasetAdmin)
            {
                user.AppRoles.Add(AppRoles.DatasetAdmin);
            }
        }

        public static Mock<IUserService> GetUserServiceMockForUserWithStudyRole(bool employee, int userId)
        {
            return SetupUserServiceMock(GetBasicAuthenticatedUser(employee, userId));
        }

        public static Mock<IUserService> GetUserServiceMockForBasicUser(bool employee, int userId)
        {
            return SetupUserServiceMock(GetBasicAuthenticatedUser(employee, userId));
        }

        public static Mock<IUserService> GetUserServiceMockForAdmin(int userId, string objectId = UserTestConstants.COMMON_CUR_USER_OBJECTID)
        {
            return  SetupUserServiceMock(GetAdmin(userId, objectId));     
        }

        public static Mock<IUserService> GetUserServiceMockForSponsor(int userId)
        {
            return SetupUserServiceMock(GetSponsor(userId));
        }

        public static Mock<IUserService> GetUserServiceMockForDatasetAdmin(int userId)
        {
            return SetupUserServiceMock(GetDatasetAdmin(userId));
        }

        public static Mock<IUserService> GetUserServiceForDatasetAdmin(int userId)
        {
            return SetupUserServiceMock(GetDatasetAdmin(userId));
        }

        public static Mock<IUserService> GetUserServiceMockForAppRole(string appRole, int userId)
        {
            switch (appRole)
            {
                case AppRoles.Admin:
                    return GetUserServiceMockForAdmin(userId);
                case AppRoles.Sponsor:
                    return GetUserServiceMockForSponsor(userId);
                case AppRoles.DatasetAdmin:
                    return GetUserServiceMockForDatasetAdmin(userId);
            }
            return SetupUserServiceMock(GetAdmin(userId));
        }

        static Mock<IUserService> SetupUserServiceMock(UserDto user)
        {
            var mock = new Mock<IUserService>();
            mock.Setup(us => us.GetCurrentUserAsync(It.IsAny<bool>())).ReturnsAsync(user);
            mock.Setup(us => us.EnsureExists(It.IsAny<UserDto>())).ReturnsAsync(user);
            return mock;
        }       
    }
}
