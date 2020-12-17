﻿using Sepes.Infrastructure.Constants;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.Queries;
using Sepes.Tests.Constants;
using Sepes.Tests.Setup;
using System.Linq;
using Xunit;

namespace Sepes.Tests.Services.DomainServices.Queries
{
    public class StudyAccessUtilTest_SingleStudy : StudyQueriesTest_Base
    {
        public StudyAccessUtilTest_SingleStudy()
            : base()
        {
        
        }

        [Fact]
        public async void ReadingUnrestrictedStudyAsEmployee_ShouldBeAllowed()
        {
            var db = GetContextWithSimpleTestData(UserConstants.COMMON_CUR_USER_DB_ID, COMMON_STUDY_ID, false);
            
            var userServiceMock = UserFactory.GetUserServiceMockForBasicUser(true, UserConstants.COMMON_CUR_USER_DB_ID);        

            var returnedStudy = await StudySingularQueries.GetStudyByIdCheckAccessOrThrow(db, userServiceMock.Object, COMMON_STUDY_ID, UserOperation.Study_Read);
            Assert.NotNull(returnedStudy); 
        }

        [Fact]
        public async void ReadingUnrestrictedStudyAsEmployee_ShouldThrow()
        {
            var db = GetContextWithSimpleTestData(UserConstants.COMMON_CUR_USER_DB_ID, COMMON_STUDY_ID, false);

            var userServiceMock = UserFactory.GetUserServiceMockForBasicUser(false, UserConstants.COMMON_CUR_USER_DB_ID);

            await Assert.ThrowsAsync<ForbiddenException>(() => StudySingularQueries.GetStudyByIdCheckAccessOrThrow(db, userServiceMock.Object, COMMON_STUDY_ID, UserOperation.Study_Read));
        }

        [Fact]  
        public async void ReadingRestrictedStudy_AsAdmin_ShouldSucceeed()
        {
            var db = GetContextWithSimpleTestData(UserConstants.COMMON_CUR_USER_DB_ID, COMMON_STUDY_ID, true);

            var userServiceMock = UserFactory.GetUserServiceMockForAdmin(UserConstants.COMMON_CUR_USER_DB_ID);

            var study = await StudySingularQueries.GetStudyByIdCheckAccessOrThrow(db, userServiceMock.Object, COMMON_STUDY_ID, UserOperation.Study_Read);

            PerformUsualStudyTests(study);
        }

        [Theory]
        [InlineData(AppRoles.Admin, StudyRoles.StudyOwner)]
        [InlineData(AppRoles.Sponsor, StudyRoles.StudyOwner)]
        public async void ReadingRestrictedStudy_WithRelevantAppRole_ShouldSucceeed(string appRole, string studySpecificRole)
        {
            var db = GetContextWithSimpleTestData(UserConstants.COMMON_CUR_USER_DB_ID, COMMON_STUDY_ID, true, studySpecificRole);

            var userServiceMock = UserFactory.GetUserServiceMockForAppRole(appRole, UserConstants.COMMON_CUR_USER_DB_ID);
       
            var study = await StudySingularQueries.GetStudyByIdCheckAccessOrThrow(db, userServiceMock.Object, COMMON_STUDY_ID, UserOperation.Study_Read);

            PerformUsualStudyTests(study);
            UserMustBeAmongStudyParticipants(study);
        }

        [Theory]
        [InlineData(AppRoles.DatasetAdmin, StudyRoles.StudyOwner)]
        public async void ReadingRestrictedStudy_WithWrongAppRole_ShouldFail(string appRole, string studySpecificRole)
        {
            var db = GetContextWithSimpleTestData(UserConstants.COMMON_CUR_USER_DB_ID, COMMON_STUDY_ID, true, studySpecificRole);

            var userServiceMock = UserFactory.GetUserServiceMockForAppRole(appRole, UserConstants.COMMON_CUR_USER_DB_ID);

            await Assert.ThrowsAsync<ForbiddenException>(() => StudySingularQueries.GetStudyByIdCheckAccessOrThrow(db, userServiceMock.Object, COMMON_STUDY_ID, UserOperation.Study_Read));
        }

        [Theory]
        [InlineData(false, StudyRoles.SponsorRep)]
        [InlineData(false, StudyRoles.VendorAdmin)]
        [InlineData(false, StudyRoles.VendorContributor)]
        [InlineData(false, StudyRoles.StudyViewer)]
        [InlineData(true, StudyRoles.SponsorRep)]
        [InlineData(true, StudyRoles.VendorAdmin)]
        [InlineData(true, StudyRoles.VendorContributor)]
        [InlineData(true, StudyRoles.StudyViewer)]
        public async void ReadingRestrictedStudyWithRelevantPermission_ShouldSucceed(bool employee, string studySpecificRole)
        {
            var db = GetContextWithSimpleTestData(UserConstants.COMMON_CUR_USER_DB_ID, COMMON_STUDY_ID, true, studySpecificRole);

            var userServiceMock = UserFactory.GetUserServiceMockForBasicUser(employee, UserConstants.COMMON_CUR_USER_DB_ID);
            var study = await StudySingularQueries.GetStudyByIdCheckAccessOrThrow(db, userServiceMock.Object, COMMON_STUDY_ID, UserOperation.Study_Read);

            PerformUsualStudyTests(study);
            UserMustBeAmongStudyParticipants(study);
        }   


        [Theory]
        [InlineData(false, "null")]
        [InlineData(false, "Not a real role")]
        [InlineData(false, "")]
        [InlineData(false, StudyRoles.StudyOwner)]
        [InlineData(true, "null")]
        [InlineData(true, "Not a real role")]
        [InlineData(true, "")]
        [InlineData(true, StudyRoles.StudyOwner)]
        public async void ReadingRestrictedStudyThatHasBogusRole_ShouldFail(bool employee, string justSomeBogusRole)
        {
            var db = GetContextWithSimpleTestData(UserConstants.COMMON_CUR_USER_DB_ID, COMMON_STUDY_ID, true, justSomeBogusRole);

            var userServiceMock = UserFactory.GetUserServiceMockForBasicUser(employee, UserConstants.COMMON_CUR_USER_DB_ID);
            await Assert.ThrowsAsync<ForbiddenException>(() => StudySingularQueries.GetStudyByIdCheckAccessOrThrow(db, userServiceMock.Object, COMMON_STUDY_ID, UserOperation.Study_Read));
        }       
    }
}