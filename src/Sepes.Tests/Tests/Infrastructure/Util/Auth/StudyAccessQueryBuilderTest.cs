using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Infrastructure.Util.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sepes.Tests.Util.Auth
{
    public class StudyAccessQueryBuilderTest
    {
        [Fact]
        public void StudyAccessQueryBuilder_ShouldContainStudyName()
        {
            var user = new UserDto { Sponsor = true };
            var result = StudyAccessQueryBuilder.CreateAccessWhereClause(user, UserOperation.Sandbox_OpenInternet);

            Assert.Contains("(1 = 1 AND sp.UserId = 0 AND sp.RoleName  = 'Study Owner') OR (1 = 1 AND sp.UserId = 0 AND sp.RoleName  IN ('Sponsor Rep','Vendor Admin'))", result);
        }

        [Fact]
        public void StudyAccessQueryBuilder_ShouldContainStudyName2()
        {
            var user = new UserDto { DatasetAdmin = true };
            var result = StudyAccessQueryBuilder.CreateAccessWhereClause(user, UserOperation.Sandbox_OpenInternet);

            Assert.Contains("(1 = 1 AND sp.UserId = 0 AND sp.RoleName  IN ('Sponsor Rep','Vendor Admin'))", result);
        }

        [InlineData(UserOperation.Study_Delete)]
        [Theory]
        public void StudyAccessQueryBuilder_ShouldContainStudyName3(UserOperation userOperation)
        {
            var user = new UserDto { DatasetAdmin = true };
            var result = StudyAccessQueryBuilder.CreateAccessWhereClause(user, userOperation);

            Assert.Null(result);
        }

        [InlineData(UserOperation.Study_Delete)]
        [Theory]
        public void StudyAccessQueryBuilder_ShouldContainStudyName4(UserOperation userOperation)
        {
            var user = new UserDto { Employee = true };
            var result = StudyAccessQueryBuilder.CreateAccessWhereClause(user, userOperation);

            Assert.Null(result);
        }
    }
}
