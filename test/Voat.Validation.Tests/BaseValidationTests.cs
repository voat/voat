using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Voat.Validation.Tests
{
    [TestClass]
    public abstract class BaseValidationTests
    {
        public Func<object, ValidationContext, List<ValidationResult>, bool, bool> _validator;

        private TestContext _testContextInstance;

        public BaseValidationTests(Func<object, ValidationContext, List<ValidationResult>, bool, bool> Validator)
        {
            this._validator = Validator;
        }

        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Basic_Invalid()
        {
            var obj = new BasicValidationObject();
            var result = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), result, true);
            Assert.IsFalse(valid);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [TestCategory("Validation")]
        //Trap: updates didn't pick up non standard inputs 2/6/14
        public void Validation_Basic_StringLength_Invalid()
        {
            var input = "This is a long string that should not pass validation so we are going to continue rambling on and on for a while meow";
            var obj = new BasicValidationObject() { StringProperty = input, EmailProperty = "valid@email.com", IntegerProperty = 3 };
            var result = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), result, true);

            Assert.IsFalse(valid, "Validation Object returned invalid for valid model");
            Assert.AreNotEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Basic_Valid()
        {
            var obj = new BasicValidationObject() { StringProperty = "Some String", EmailProperty = "valid@email.com", IntegerProperty = 3 };
            var result = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), result, true);

            Assert.IsTrue(valid, "Validation Object returned invalid for valid model");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.CustomAttribute")]
        public void Validation_CustomAttribute_Fail()
        {
            var obj = new CustomObjectFails();
            var result = new List<ValidationResult>();
            var isvalid = _validator(obj, new ValidationContext(obj), result, true);

            Assert.IsFalse(isvalid);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.CustomAttribute")]
        public void Validation_CustomAttribute_Pass()
        {
            var obj = new CustomObjectPass();
            var result = new List<ValidationResult>();
            var isvalid = _validator(obj, new ValidationContext(obj), result, true);

            Assert.IsTrue(isvalid);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.CustomAttribute")]
        public void Validation_CustomAttributeFail_Conditional()
        {
            var obj = new CustomObjectConditional();
            obj.Pass = true;
            var result = new List<ValidationResult>();
            var isvalid = _validator(obj, new ValidationContext(obj), result, true);

            Assert.IsFalse(isvalid);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.CustomAttribute")]
        public void Validation_CustomAttributePass_Conditional()
        {
            var obj = new CustomObjectConditional();
            obj.Pass = false;
            var result = new List<ValidationResult>();
            var isvalid = _validator(obj, new ValidationContext(obj), result, true);
            Assert.IsTrue(isvalid);
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Nested_List_Cast_To_BaseType()
        {
            var input = "This is a long string that should not pass validation so we are going to continue rambling on and on for a while meow";
            var obj = new NestedListValidationObject();
            obj.NestedObject.Add(new BasicValidationObject() { StringProperty = input, EmailProperty = "invalid email", IntegerProperty = 45 });
            obj.NestedObject.Add(new ExtendedBasicValidationObject() { StringProperty = input, EmailProperty = "invalid email", IntegerProperty = 45 });
            obj.GuidID = "Not A Guid";

            var results = ValidationHandler.Validate(obj);

            Assert.AreEqual("NestedObject[1].ExtendedProperty", results[4].MemberNames.First());
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Nested_List_Invalid()
        {
            var input = "This is a long string that should not pass validation so we are going to continue rambling on and on for a while meow";
            var obj = new NestedListValidationObject();
            obj.NestedObject.Add(new BasicValidationObject() { StringProperty = input, EmailProperty = "invalid email", IntegerProperty = 45 });
            obj.GuidID = Guid.NewGuid().ToString();

            var r = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), r, true);

            if (this.GetType() == typeof(SystemValidationTests))
            {
                Assert.IsTrue(valid);
            }
            else
            {
                Assert.IsFalse(valid);
            }
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Nested_List_Null()
        {
            var obj = new NestedListValidationObject();
            obj.NestedObject = null;
            obj.GuidID = Guid.NewGuid().ToString();

            var r = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), r, true);

            Assert.IsFalse(valid);
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Nested_Object_Invalid()
        {
            var input = "This is a long string that should not pass validation so we are going to continue rambling on and on for a while meow";
            var obj = new NestedValidationObject();
            obj.NestedObject = new BasicValidationObject() { StringProperty = input, EmailProperty = "invalid email", IntegerProperty = 45 };
            obj.GuidID = "Not A Guid";

            var r = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), r, true);

            Assert.IsFalse(valid);
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Nested_Object_Null()
        {
            var obj = new NestedValidationObject();
            obj.NestedObject = null;
            obj.GuidID = Guid.NewGuid().ToString();

            var r = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), r, true);

            Assert.IsFalse(valid);
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_Nested_Valid()
        {
            var obj = new NestedValidationObject();
            obj.NestedObject = new BasicValidationObject() { StringProperty = "Some String", EmailProperty = "valid@email.com", IntegerProperty = 3 };
            obj.GuidID = Guid.NewGuid().ToString();

            var r = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), r, true);

            Assert.IsTrue(valid);
            Assert.AreEqual(0, r.Count);
        }

        [TestMethod]
        [TestCategory("Validation")]
        public void Validation_NoValidation()
        {
            var obj = new NoValidationObject();
            var result = new List<ValidationResult>();
            var valid = _validator(obj, new ValidationContext(obj), result, true);
            Assert.IsTrue(valid);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.CustomAttribute")]
        public void Validation_Validateable_ObjectFail()
        {
            var obj = new ValidatableObject();
            obj.Pass = false;
            var result = new List<ValidationResult>();
            var isvalid = _validator(obj, new ValidationContext(obj), result, true);
            Assert.AreEqual(obj.Pass, isvalid);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.CustomAttribute")]
        public void Validation_Validateable_ObjectPass()
        {
            var b = new ValidatableObject();
            b.Pass = true;
            var r = new List<ValidationResult>();
            var isvalid = _validator(b, new ValidationContext(b), r, true);
            Assert.AreEqual(b.Pass, isvalid);
        }
    }

    [TestClass]
    public class SystemValidationTests : BaseValidationTests
    {
        public SystemValidationTests()
            : base(new Func<object, ValidationContext, List<ValidationResult>, bool, bool>((o, v, l, b) =>
            {
                return Validator.TryValidateObject(o, v, l, b);
            }))
        {
        }
    }

    [TestClass]
    public class VoatValidationTests : BaseValidationTests
    {
        public VoatValidationTests()
            : base(new Func<object, ValidationContext, List<ValidationResult>, bool, bool>((o, v, l, b) =>
            {
                var result = ValidationHandler.Validate(o);

                if (result != null)
                {
                    result.ToList().ForEach(x => l.Add(new ValidationResult(x.ErrorMessage, new List<string> { x.MemberNames.First() })));
                }

                return l.Count() == 0;
            }))
        { }
    }
}