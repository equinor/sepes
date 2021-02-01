﻿using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sepes.Tests.Util
{
    public class GenericNameValidationTest
    {
        [Theory]
        [InlineData("A valid study Name")]
        [InlineData("Valid123")]
        [InlineData("     Valid123         ")]
        public void GenericNameValidationTest_ShouldNotReturn(string name)
        {
            GenericNameValidation.ValidateName(name);
        }

        [Theory]
        [InlineData("Not Valid!!!")]
        [InlineData("")]
        [InlineData("AB")]
        [InlineData("       ")]
        public void GenericNameValidationTest_ShouldThrow(string name)
        {
            Assert.Throws<System.ArgumentException>(() => GenericNameValidation.ValidateName(name));
        }
    }
}