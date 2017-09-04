using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Voat.Validation.Tests.TestObjects
{
   
    public class Nested<T> //: IValidatableObject
    {
        //[Required]
        public string UserName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();

            var errors = new List<ValidationResult>();
            //errors.Add(new ValidationResult("Error From NestedSubType", new[] { "IMadeThisUp" }));
            //if (String.IsNullOrEmpty(UserName))
            //{
            //    errors.Add(new ValidationResult("Error From NestedSubType", new[] { "UserName" }));
            //    //errors.Add(ValidationPathResult.Create(this, "Error From NestedSubType", m => m.UserName));
            //}

            return errors;
        }

        [PerformValidation]
        public List<T> Options { get; set; } = new List<T>();
    }

    public class NestedSubTypeBase { }
    
    public class NestedSubType : NestedSubTypeBase, IValidatableObject
    {
        public string UserName { get; set; }

        public bool UseValidationPathResult { get; set; } = true;
        public bool ReturnValidationError { get; set; } = true;

        public List<NestedSubType> EndlessNesting { get; set; } = new List<NestedSubType>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
            if (ReturnValidationError)
            {
                if (UseValidationPathResult)
                {
                    errors.Add(ValidationPathResult.Create(this, "Invalid: {0}", m => m.UserName));
                }
                else
                {
                    errors.Add(new ValidationResult("Invalid: {0}", new[] { "UserName" }));
                }
            }
            return errors;
        }

    }
}
