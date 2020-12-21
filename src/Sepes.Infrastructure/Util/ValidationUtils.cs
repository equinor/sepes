﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sepes.Infrastructure.Util
{
    public static class ValidationUtils
    {
        public static void ThrowIfValidationErrors(string messagePrefix, List<string> validationErrors)
        {
            if (validationErrors.Count > 0)
            {
                var validationErrorMessageBuilder = new StringBuilder();

                foreach (var curValidation in validationErrors)
                {
                    validationErrorMessageBuilder.AppendLine(curValidation);
                }

                throw new Exception($"{messagePrefix}: {validationErrorMessageBuilder}");
            }
        }
    }
}
