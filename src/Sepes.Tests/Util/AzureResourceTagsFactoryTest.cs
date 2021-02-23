﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using Sepes.Tests.Services;
using Sepes.Tests.Setup;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sepes.Tests.Util
{
    public class AzureResourceTagsFactoryTest : ServiceTestBase
    {
        [Fact]
        public void SandboxResourceTags_ShouldReturnCorrectValues()
        {

            var config = AzureResourceTagsFactory_Factory.GetConfiguration(_serviceProvider);
            List<StudyParticipant> participants = new List<StudyParticipant>();
            var user = new User() { Id = 1, FullName = "John Doe", EmailAddress = "John@doe.com" };
            participants.Add(new StudyParticipant() { RoleName = "Study Owner", User = user });
            var study = new Study() { Name = "Study1", WbsCode = "123", StudyParticipants = participants };
            var sandbox = new Sandbox() { Study = study, Name = "Sandbox1" };

            var res = AzureResourceTagsFactory.SandboxResourceTags(config, study, sandbox);

            var expectedResultStudy = "Study1";
            var expectedResultOwnerName = "John Doe";
            var expectedResultOwnerEmail = "John@doe.com";
            var expectedResultSandbox = "Sandbox1";

            Assert.Equal(expectedResultStudy, res["StudyName"]);
            Assert.Equal(expectedResultOwnerName, res["StudyOwnerName"]);
            Assert.Equal(expectedResultOwnerEmail, res["StudyOwnerEmail"]);
            Assert.Equal(expectedResultSandbox, res["SandboxName"]);
        }

        [Fact]
        public void StudySpecificDatasourceResourceGroupTags_ShouldReturnCorrectValues()
        {

            var config = AzureResourceTagsFactory_Factory.GetConfiguration(_serviceProvider);
            List<StudyParticipant> participants = new List<StudyParticipant>();
            var user = new User() { Id = 1, FullName = "John Doe", EmailAddress = "John@doe.com" };
            participants.Add(new StudyParticipant() { RoleName = "Study Owner", User = user });
            var study = new Study() { Name = "Study1", WbsCode = "123", StudyParticipants = participants };

            var res = AzureResourceTagsFactory.StudySpecificDatasourceResourceGroupTags(config, study);

            var expectedResultStudy = "Study1";
            var expectedResultOwnerName = "John Doe";
            var expectedResultOwnerEmail = "John@doe.com";

            Assert.Equal(expectedResultStudy, res["StudyName"]);
            Assert.Equal(expectedResultOwnerName, res["StudyOwnerName"]);
            Assert.Equal(expectedResultOwnerEmail, res["StudyOwnerEmail"]);
        }

        [Fact]
        public void StudySpecificDatasourceStorageAccountTags_ShouldReturnCorrectValues()
        {

            var config = AzureResourceTagsFactory_Factory.GetConfiguration(_serviceProvider);
            List<StudyParticipant> participants = new List<StudyParticipant>();
            var user = new User() { Id = 1, FullName = "John Doe", EmailAddress = "John@doe.com" };
            participants.Add(new StudyParticipant() { RoleName = "Study Owner", User = user });
            var study = new Study() { Name = "Study1", WbsCode = "123", StudyParticipants = participants };

            var res = AzureResourceTagsFactory.StudySpecificDatasourceStorageAccountTags(config, study, "dataset1");

            var expectedResultStudy = "Study1";
            var expectedResultOwnerName = "John Doe";
            var expectedResultOwnerEmail = "John@doe.com";
            var expectedResultDatasetName = "dataset1";

            Assert.Equal(expectedResultStudy, res["StudyName"]);
            Assert.Equal(expectedResultOwnerName, res["StudyOwnerName"]);
            Assert.Equal(expectedResultOwnerEmail, res["StudyOwnerEmail"]);
            Assert.Equal(expectedResultDatasetName, res["DatasetName"]);
        }
    }
}