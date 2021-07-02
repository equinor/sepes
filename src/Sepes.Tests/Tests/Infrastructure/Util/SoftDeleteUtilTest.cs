using Sepes.Common.Dto;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using System;
using Xunit;

namespace Sepes.Tests.Util
{
    public class SoftDeleteUtilTest
    {
        [Fact]
        public void MarkAsDeleted_ShouldMarkAsDeletedWithCorrectName()
        {
            var deletedBy = "John";
            var itemToBeDeleted = new CloudResource { };
            SoftDeleteUtil.MarkAsDeleted(itemToBeDeleted, deletedBy);

            Assert.Equal(itemToBeDeleted.DeletedBy, deletedBy);
            Assert.True(SoftDeleteUtil.IsMarkedAsDeleted(itemToBeDeleted));
        }

        [Fact]
        public void MarkAsDeleted_ShouldMarkAsDeletedWithCorrectName2()
        {
            var deletedBy = new User { UserName = "John" };
            var itemToBeDeleted = new CloudResource { };
            SoftDeleteUtil.MarkAsDeleted(itemToBeDeleted, deletedBy);

            Assert.Equal(itemToBeDeleted.DeletedBy, deletedBy.UserName);
            Assert.True(SoftDeleteUtil.IsMarkedAsDeleted(itemToBeDeleted));
        }

        [Fact]
        public void MarkAsDeleted_ShouldMarkAsDeletedWithCorrectName3()
        {
            var deletedBy = new UserDto { UserName = "John" };
            var itemToBeDeleted = new CloudResource { };
            SoftDeleteUtil.MarkAsDeleted(itemToBeDeleted, deletedBy);

            Assert.Equal(itemToBeDeleted.DeletedBy, deletedBy.UserName);
            Assert.True(SoftDeleteUtil.IsMarkedAsDeleted(itemToBeDeleted));
        }

        [Fact]
        public void MarkAsDeleted_ShouldMarkAsDeletedWithCorrectName4()
        {
            var deletedBy = new UserDto {  };
            var itemToBeDeleted = new CloudResource { };
            SoftDeleteUtil.MarkAsDeleted(itemToBeDeleted, deletedBy);

            Assert.Equal(itemToBeDeleted.DeletedBy, deletedBy.UserName);
            Assert.True(SoftDeleteUtil.IsMarkedAsDeleted(itemToBeDeleted));
        }

        [Fact]
        public void MarkAsDeleted_ShouldMarkAsDeletedWithCorrectName5()
        {
            var deletedBy = new UserDto { };
            var itemToBeDeleted = new CloudResource { };
            var ex = Assert.Throws<ArgumentNullException>(() => SoftDeleteUtil.MarkAsDeleted(null, ""));

            Assert.Contains("Item to delete was null", ex.Message);
            Assert.False(SoftDeleteUtil.IsMarkedAsDeleted(itemToBeDeleted));
        }
    }
}
