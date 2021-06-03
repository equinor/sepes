﻿using Moq;
using Sepes.Common.Interface;

namespace Sepes.Tests.Mocks
{
    public static class PrincipalServiceMock
    {
        public static Mock<IContextUserService> GetService(bool admin = false, bool sponsor = false, bool datasetAdmin = false, bool employee = false)
        {
            var currentUserServiceMock = new Mock<IContextUserService>();

            currentUserServiceMock.Setup(us => us.IsAdmin()).Returns(admin);
            currentUserServiceMock.Setup(us => us.IsSponsor()).Returns(sponsor);
            currentUserServiceMock.Setup(us => us.IsDatasetAdmin()).Returns(datasetAdmin);
            currentUserServiceMock.Setup(us => us.IsEmployee()).Returns(employee);

            return currentUserServiceMock;
        }
    }
}
