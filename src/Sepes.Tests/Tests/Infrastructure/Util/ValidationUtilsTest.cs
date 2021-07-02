using Sepes.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sepes.Tests.Util
{
    public class ValidationUtilsTest
    {
        [Fact]
        public void ThrowIfValidationErrors_NoErrorsShouldRun()
        {
            var errors = new List<string>();
            ValidationUtils.ThrowIfValidationErrors("prefix", errors);
        }

        [Fact]
        public void ThrowIfValidationErrors_NoErrorsShouldThrow()
        {
            ValidationUtils.ThrowIfValidationErrors("prefix", null);
        }

        [Fact]
        public void ThrowIfValidationErrors_ShouldThrowWithCorrectError()
        {
            var errors = new List<string>();
            errors.Add("Error1");
            errors.Add("Error2");
            var ex = Assert.Throws<Exception>(() => ValidationUtils.ThrowIfValidationErrors("prefix", errors));
            Assert.Contains("prefix", ex.Message);
            Assert.Contains("Error1", ex.Message);
            Assert.Contains("Error2", ex.Message);
        }
    }
}
