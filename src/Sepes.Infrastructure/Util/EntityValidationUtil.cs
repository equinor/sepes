using Sepes.Infrastructure.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sepes.Infrastructure.Service
{
    public static class EntityValidationUtil
    { 
        public static bool Validate<TModel>(TModel entity) where TModel : BaseModel
        {
            var validationErrors = new List<ValidationResult>();
            var context = new ValidationContext(entity, null, null);
            var isValid = Validator.TryValidateObject(entity, context, validationErrors);

            if (!isValid)
            {
                var errorBuilder = new StringBuilder();

                errorBuilder.AppendLine("Invalid data: ");

                foreach (var error in validationErrors)
                {
                    errorBuilder.AppendLine(error.ErrorMessage);
                }

                throw new ArgumentException(errorBuilder.ToString());
            }

            return true;
        }
    }
}
