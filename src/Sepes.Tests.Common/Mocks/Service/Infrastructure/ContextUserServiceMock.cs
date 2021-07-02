using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Common.Interface;
using Sepes.Tests.Common.Constants;

namespace Sepes.RestApi.IntegrationTests.Services
{
    public class ContextUserServiceMock : IContextUserService
    {
        readonly bool _isEmployee;
        readonly bool _isAdmin;
        readonly bool _isSponsor;
        readonly bool _isDatasetAdmin;

        public ContextUserServiceMock(bool isEmployee = false, bool isAdmin = false, bool isSponsor = false, bool isDatasetAdmin = false)
        {
            _isEmployee = isEmployee;
            _isAdmin = isAdmin;
            _isSponsor = isSponsor;
            _isDatasetAdmin = isDatasetAdmin;
        }

        public UserDto GetCurrentUser()
        {
           var user = new UserDto(UserTestConstants.COMMON_CUR_USER_OBJECTID, UserTestConstants.COMMON_CUR_USER_UPN, UserTestConstants.COMMON_CUR_USER_FULL_NAME, UserTestConstants.COMMON_CUR_USER_EMAIL,
                _isAdmin, _isSponsor, _isDatasetAdmin, _isEmployee);

            ApplyExtendedProps(user);

            return user;
        }
       

        void ApplyExtendedProps(UserDto user)
        {
            if (_isAdmin)
            {
                user.Admin = true;
                user.AppRoles.Add(AppRoles.Admin);
            }

            if (_isSponsor)
            {
                user.Sponsor = true;
                user.AppRoles.Add(AppRoles.Sponsor);
            }

            if (_isDatasetAdmin)
            {
                user.DatasetAdmin = true;
                user.AppRoles.Add(AppRoles.DatasetAdmin);
            }

            if (_isEmployee)
            {
                user.Employee = true;
            }
        }

        public string GetCurrentUserObjectId()
        {
           return UserTestConstants.COMMON_CUR_USER_OBJECTID;
        }

        public bool IsMockUser()
        {
            return false;
        }
    }
}
