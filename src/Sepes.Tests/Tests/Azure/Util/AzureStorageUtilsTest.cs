using Sepes.Azure.Dto;
using Sepes.Azure.Util;
using System;
using Xunit;

namespace Sepes.Tests.Util
{
    public class AzureStorageUtilsTest
    {
        [Fact]
        public void GetKeyValueFromConnectionString_ShouldReturnExpected()
        {
            var result = AzureStorageUtils.GetKeyValueFromConnectionString("aa=aa;aaa=aa", "aa");

            Assert.Equal("aa", result);
        }

        [Fact]
        public void GetKeyValueFromConnectionString_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => AzureStorageUtils.GetKeyValueFromConnectionString(null, null));
        }

        [InlineData("aaaaa", "aaaaa")]
        [InlineData(";:", ":;")]
        [InlineData("a", "")]
        [Theory]
        public void GetKeyValueFromConnectionString_ShouldThrow2(string connectionString, string key)
        {
            var ex = Assert.Throws<ArgumentException>(() => AzureStorageUtils.GetKeyValueFromConnectionString(connectionString, key));
            Assert.Equal("Connection string in wrong format", ex.Message);
        }

        [Fact]
        public void GetAccountName_ShouldThrow()
        {
            Assert.Throws<NullReferenceException>(() => AzureStorageUtils.GetAccountName(null));
        }

        [Fact]
        public void GetAccountName_ShouldThrow2()
        {
            var ex = Assert.Throws<ArgumentException>(() => AzureStorageUtils.GetAccountName(AzureStorageAccountConnectionParameters.CreateUsingConnectionString("")));
            Assert.Equal("Neither connection string or account name specified", ex.Message);
        }
        [Fact]
        public void GetAccountName_ShouldThrow3()
        {
            var ex = Assert.Throws<ArgumentException>(() => AzureStorageUtils.GetAccountName(AzureStorageAccountConnectionParameters.CreateUsingConnectionString("aaaaa")));
            Assert.Equal("Connection string in wrong format", ex.Message);
        }
        [Fact]
        public void GetAccountName_ShouldReturn()
        {
            var result = AzureStorageUtils.GetAccountName(AzureStorageAccountConnectionParameters.CreateUsingConnectionString("AccountName=This is the account name;aaa=aa"));
            Assert.Equal("This is the account name", result);
        }
        [Fact]
        public void GetAccountName_ShouldReturn2()
        {
            var result = AzureStorageUtils.GetAccountName(AzureStorageAccountConnectionParameters.CreateUsingResourceGroupAndAccountName("resourceGroup", "accountName"));
            Assert.Equal("accountName", result);
        }
    }
}
