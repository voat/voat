using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Voat.Validation.Tests
{
    public class BasicValidationObject
    {
        [Range(0.0, 10.0)]
        public double DoubleProperty { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddressAttribute]
        public string EmailProperty { get; set; }

        [Required]
        [Range(1, 10)]
        public int IntegerProperty { get; set; }

        [Required]
        [StringLength(20)]
        public string StringProperty { get; set; }
        #region Gets

        public static BasicValidationObject Invalid
        {
            get
            {
                return new BasicValidationObject();
            }
        }

        public static BasicValidationObject Valid
        {
            get
            {
                return new BasicValidationObject() { StringProperty = "Some String", EmailProperty = "valid@email.com", IntegerProperty = 3 };
            }
        }
        #endregion Gets
    }

    [CustomValidation(typeof(ObjectValidator), "Conditional")]
    public class CustomObjectConditional : PassFail
    {
    }

    [CustomValidation(typeof(ObjectValidator), "FailTest")]
    public class CustomObjectFails
    {
    }

    [CustomValidation(typeof(ObjectValidator), "PassTest")]
    public class CustomObjectPass
    {
    }

    public class ExtendedBasicValidationObject : BasicValidationObject
    {
        [Required]
        [Range(1, 10)]
        public int ExtendedProperty { get; set; }
    }

    public class NestedDictionaryValidationObject<D>
    {
        public NestedDictionaryValidationObject()
        {
            NestedDictionary = new Dictionary<D, BasicValidationObject>();
        }

        [Required]
        [RegularExpression(@"[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}")]
        public string GuidID { get; set; }

        [Required]
        public Dictionary<D, BasicValidationObject> NestedDictionary { get; set; }
    }

    public class NestedEndlessListValidationObject
    {
        public NestedEndlessListValidationObject()
        {
        }

        [Required]
        [RegularExpression(@"[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}")]
        public string GuidID { get; set; }

        [Required]
        public List<NestedListValidationObject> NestedObject { get; set; }
    }

    public class NestedListValidationObject
    {
        public NestedListValidationObject()
        {
            NestedObject = new List<BasicValidationObject>();
        }

        [Required]
        [RegularExpression(@"[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}")]
        public string GuidID { get; set; }

        [Required]
        public List<BasicValidationObject> NestedObject { get; set; }

        #region Gets

        public static NestedListValidationObject Invalid
        {
            get
            {
                var n = new NestedListValidationObject();
                n.NestedObject.Add(BasicValidationObject.Invalid);
                n.NestedObject.Add(BasicValidationObject.Invalid);
                return n;
            }
        }

        public static NestedListValidationObject Valid
        {
            get
            {
                var n = new NestedListValidationObject();
                n.GuidID = Guid.NewGuid().ToString();
                n.NestedObject.Add(BasicValidationObject.Valid);
                n.NestedObject.Add(BasicValidationObject.Valid);
                return n;
            }
        }
        #endregion Gets
    }

    public class NestedValidationObject
    {
        public NestedValidationObject()
        {
            NestedObject = new BasicValidationObject();
        }

        [Required]
        [RegularExpression(@"[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}")]
        public string GuidID { get; set; }

        [Required]
        public BasicValidationObject NestedObject { get; set; }

        #region Gets

        public static NestedValidationObject Invalid
        {
            get
            {
                return new NestedValidationObject();
            }
        }

        public static NestedValidationObject Valid
        {
            get
            {
                var n = new NestedValidationObject();
                n.GuidID = Guid.NewGuid().ToString();
                n.NestedObject = BasicValidationObject.Valid;
                return n;
            }
        }
        #endregion Gets
    }

    public class NoValidationObject
    {
        public double MyDouble { get; set; }
        public GCCollectionMode MyGCCollectionMode { get; set; }
        public int MyInt { get; set; }
        public string MyString { get; set; }
    }

    public class ObjectValidator
    {
        public static ValidationResult Conditional(object o, ValidationContext context)
        {
            CustomObjectConditional x = o as CustomObjectConditional;
            if (x == null || x.Pass)
            {
                return FailTest(o, context);
            }
            else
            {
                return PassTest(o, context);
            }
        }

        public static ValidationResult FailTest(object o, ValidationContext context)
        {
            return new ValidationResult("ObjectValidator returned False", new List<string> { "Fail" });
        }

        public static ValidationResult PassTest(object o, ValidationContext context)
        {
            return ValidationResult.Success;
        }
    }

    #region Voat.Validation Support Objects

    [CustomValidation(typeof(CustomObjectConditional), "Pass")]
    [DataValidation(typeof(UnitTestValidation), "Simple", "Validate", -1, ErrorMessage = "Simple.BeforeSave")]
    [DataValidation(typeof(UnitTestValidation), "Simple", "BeforeSave", -1, ErrorMessage = "Simple.BeforeSave")]
    [DataValidation(typeof(UnitTestValidation), "Complex", "BeforeSave", -1, ErrorMessage = "Simple.BeforeSave")]
    [DataValidation(typeof(UnitTestValidation))]
    [CustomValidation(typeof(CustomObjectConditional), "Fail")]
    public class FilteredObject
    {
        public string Name { get; set; }
    }

    public class UniqueIDNameValidatorTest : DataValidator<Object>
    {
        public UniqueIDNameValidatorTest(string ruleID) : base(ruleID)
        {
        }

        public override IEnumerable<ValidationPathResult> Validate(Object value, ValidationContext context)
        {
            return null;
        }
    }

    [DataValidation(typeof(UnitTestValidation))]
    public class UnitTestModel
    {
        public string Name { get; set; }
    }
    public class UnitTestValidation : DataValidator<UnitTestModel>
    {
        public override IEnumerable<ValidationPathResult> Validate(UnitTestModel value, ValidationContext context)
        {
            if (String.Equals("<forbidden phrase>", value.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationPathResult.Create(value, "Property '{0}' with value '" + value.Name + "' is forbidden in this environment", x => x.Name).ToEnumerable();
            }
            return null;
        }
    }
    #endregion Voat.Validation Support Objects
    public class PassFail
    {
        public bool Pass { get; set; }
    }

    public class ValidatableObject : PassFail, IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Pass)
            {
                return new List<ValidationResult> { };
            }
            else
            {
                return new List<ValidationResult> { new ValidationResult("You SHALL not PASS") };
            }
        }
    }
    #region LateBound Validation Loader Types

    public class ValidateID : DataValidator<DomainObject17>
    {
        public override IEnumerable<ValidationPathResult> Validate(DomainObject17 value, ValidationContext context)
        {
            if (value.ID <= 0)
            {
                return ValidationPathResult.Create(value, "ID can't be less than zero", x => x.ID).ToEnumerable();
            }
            return null;
        }
    }

    #endregion LateBound Validation Loader Types

    #region Pipeline & Stage objects

    [DataValidation(typeof(PipelineValidator), null, "Update", 0)]
    [DataValidation(typeof(PipelineValidator), "Pipeline1", "Update", 0)]
    [DataValidation(typeof(PipelineValidator), "Pipeline2", "Update", 34)]
    [DataValidation(typeof(PipelineValidator), "Pipeline3", "Update", -73)]
    [DataValidation(typeof(PipelineValidator), "", "Delete", 0)]
    [DataValidation(typeof(PipelineValidator), "", "Create", 2)]
    public class PipelineStageTestModel : PassFail
    {
        public PipelineStageTestModel(bool shouldPass)
        {
            base.Pass = shouldPass;
        }
    }

    [DataValidation(typeof(PipelineValidator), "Pipeline2", "Update", 4)]
    [DataValidation(typeof(PipelineValidator), "Pipeline1", "Update", 0)]
    public class PipelineStateTestModelDerived : PipelineStageTestModel
    {
        public PipelineStateTestModelDerived(bool shouldPass) : base(shouldPass)
        {
        }
    }

    public class PipelineValidator : DataValidator<PipelineStageTestModel>
    {
        public override IEnumerable<ValidationPathResult> Validate(PipelineStageTestModel value, ValidationContext context)
        {
            if (!value.Pass)
            {
                return ValidationPathResult.Create(value, "{0} didn't pass yo!", x => x.Pass).ToEnumerable();
            }
            return null;
        }
    }
    #endregion Pipeline & Stage objects
}