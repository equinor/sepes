using Sepes.Common.Util;
using Xunit;

namespace Sepes.Tests.Util
{
    public class ConfigUtilTest
    {
        [Theory]
        [InlineData("Server=someserver;Uid=someuserid;Pwd=myverysecretpassword;Database=sepes;", "Server=someserver;Uid=someuserid;Pwd=<password removed>;Database=sepes;")]
        [InlineData("Server=someserver;Uid=someuserid;pwd=myverysecretpassword;Database=sepes;", "Server=someserver;Uid=someuserid;pwd=<password removed>;Database=sepes;")] //Same with lower case
        [InlineData("Server=someserver;Uid=someuserid;Password=myverysecretpassword;Database=sepes;", "Server=someserver;Uid=someuserid;Password=<password removed>;Database=sepes;")]
        [InlineData("Server=someserver;Uid=someuserid;password=myverysecretpassword;Database=sepes;", "Server=someserver;Uid=someuserid;password=<password removed>;Database=sepes;")] //Same with lower case
        public void RemovePasswordFromConnectionString_ShouldOnlyRemovePasswordPart(string connectionString, string expectedResult)
        {           
           var cleanedConfigString = ConfigUtil.RemovePasswordFromConnectionString(connectionString);
            Assert.Equal(expectedResult, cleanedConfigString);
        }

       
    }
}
