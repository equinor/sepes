﻿using Sepes.Infrastructure.Util;
using Xunit;

namespace Sepes.Tests.Util
{

    public class AzureResourceNameUtilTest
    {
        [Fact]
        public void ResourceGroupName_ShouldContainStudyAndSandboxName()
        {
            var resourceGroupName = AzureResourceNameUtil.ResourceGroup("Very Secret Software Inc", "First attempt at finding good data");
            Assert.InRange(resourceGroupName.Length, 6, 64);
         
            Assert.Contains("rg-study-", resourceGroupName);
            Assert.Contains("verysecretsoftwareinc", resourceGroupName);
            Assert.Contains("firstattemptatfindinggooddata", resourceGroupName);

        }

        [Fact]
        public void ResourceGroupName_ShouldNotExceed64Characters()
        {
            var resourceGroupName = AzureResourceNameUtil.ResourceGroup("Very Very Secret Software Inc", "First attempt at finding good data");
            Assert.InRange(resourceGroupName.Length, 6, 64);
            Assert.Contains("rg-study-", resourceGroupName);
            Assert.Contains("veryverysecretsoftwareinc", resourceGroupName);
            Assert.Contains("firstattemptatfindinggood", resourceGroupName);
        }

        [Fact]
        public void DiagStorageAccountName_ShouldNotExceed24Characters()
        {
            var resourceName = AzureResourceNameUtil.DiagnosticsStorageAccount("Bestest Study Ever", "Strezztest1");
            Assert.InRange(resourceName.Length, 4, 24);
            Assert.Contains("stdiag", resourceName);
            Assert.Contains("bestes", resourceName);
            Assert.Contains("strezzte", resourceName);


            var resourceName2 = AzureResourceNameUtil.DiagnosticsStorageAccount("Bestest Study Ever", "The third test we are going to too");
            Assert.InRange(resourceName2.Length, 4, 24);
            Assert.Contains("stdiag", resourceName2);
            Assert.Contains("bestest", resourceName2);
            Assert.Contains("thethird", resourceName2);

        }

        [Fact]
        public void ResourceGroupName_ShouldFilterAwayNorwegianSpecialLetters()
        {
            var resourceName = AzureResourceNameUtil.ResourceGroup("A revolutional Støddy with a long name", "Bæste sandbåx ju kæn tink");
            Assert.InRange(resourceName.Length, 4, 64);            
            Assert.Contains("rg-study-arevolutionalstddywithalongname-bstesandbxjukntink-", resourceName);
        }

        [Fact]
        public void DiagStorageAccountName_ShouldFilterAwayNorwegianSpecialLetters()
        {
            var resourceName = AzureResourceNameUtil.DiagnosticsStorageAccount("Støddy", "Bæste sandbåx");
            Assert.InRange(resourceName.Length, 4, 24);
     
            Assert.Contains("stdiagstddybstesandbx", resourceName);
   
        }

        }
}
