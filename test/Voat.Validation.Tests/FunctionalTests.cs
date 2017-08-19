using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Voat.Common.Fs;

namespace Voat.Validation.Tests
{
    [TestClass]
    public class FunctionalTests
    {
        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validate_BasicObject_Invalid()
        {
            var obj = new BasicValidationObject();
            var result = ValidationHandler.Validate(obj);
            Assert.IsTrue(result.Count() > 0);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("Required", ((ValidationPathResult)result[0]).Type);
            Assert.AreEqual("Range", ((ValidationPathResult)result[1]).Type);
            Assert.AreEqual("Required", ((ValidationPathResult)result[2]).Type);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validate_BasicObject_Valid()
        {
            var obj = new BasicValidationObject() { StringProperty = "Some String", EmailProperty = "valid@email.com", IntegerProperty = 3 };
            var result = ValidationHandler.Validate(obj);

            Assert.IsTrue(result == null);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validate_NestedDictionaryObject_Invalid()
        {
            var obj = new NestedDictionaryValidationObject<int>();
            obj.GuidID = Guid.NewGuid().ToString();
            obj.NestedDictionary.Add(1, BasicValidationObject.Valid);
            obj.NestedDictionary.Add(13, BasicValidationObject.Invalid);
            var r = ValidationHandler.Validate(obj);
            Assert.IsNotNull(r);
            Assert.IsNotNull(r.FirstOrDefault(x => x.MemberNames.Any(y => y == "NestedDictionary[13].EmailProperty")));
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validate_NestedDictionaryObject_Invalid2()
        {
            var obj = new NestedDictionaryValidationObject<string>();
            obj.GuidID = Guid.NewGuid().ToString();
            obj.NestedDictionary.Add("1", BasicValidationObject.Valid);
            obj.NestedDictionary.Add("13", BasicValidationObject.Invalid);
            var r = ValidationHandler.Validate(obj);
            Assert.IsNotNull(r);
            Assert.IsNotNull(r.FirstOrDefault(x => x.MemberNames.Any(y => y == "NestedDictionary[\"13\"].EmailProperty")));
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validate_NestedListObject_Invalid()
        {
            var obj = new NestedListValidationObject();
            obj.NestedObject = new List<BasicValidationObject>();
            obj.NestedObject.Add(BasicValidationObject.Valid);
            obj.NestedObject.Add(BasicValidationObject.Invalid);
            obj.NestedObject.Add(BasicValidationObject.Valid);
            var r = ValidationHandler.Validate(obj);
            Assert.IsTrue(r.Count() > 0);
            Assert.AreEqual(4, r.Count());
            Assert.IsNotNull(r.FirstOrDefault(x => x.MemberNames.Any(y => y == "NestedObject[1].EmailProperty")));
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validate_NestedObject_Invalid()
        {
            var obj = new NestedValidationObject();
            obj.NestedObject = new BasicValidationObject();
            //n.NestedObject.SubNest = null;
            var r = ValidationHandler.Validate(obj);
            Assert.IsTrue(r.Count() > 0);
            Assert.AreEqual(4, r.Count());
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validate_NestedObject_Valid()
        {
            var obj = new NestedValidationObject();
            obj.GuidID = Guid.NewGuid().ToString();
            obj.NestedObject = new BasicValidationObject() { StringProperty = "Some String", EmailProperty = "valid@email.com", IntegerProperty = 3 };
            var r = ValidationHandler.Validate(obj);
            Assert.IsTrue(r == null);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validation_FindAttributes()
        {
            var found = AttributeFinder.Find<ValidationAttribute>(typeof(BasicValidationObject));
            Assert.IsTrue(found.Count == 1);
            Assert.IsTrue(found.First().Value.Count == 4);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validation_FindAttributesWithNested()
        {
            var found = AttributeFinder.Find<ValidationAttribute>(typeof(NestedValidationObject));
            Assert.IsTrue(found.Count == 2);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validation_FindAttributesWithNestedList()
        {
            var found = AttributeFinder.Find<ValidationAttribute>(typeof(NestedListValidationObject));
            Assert.IsTrue(found.Count == 2);
            Assert.IsTrue(found[typeof(NestedListValidationObject)].Count == 2);
            Assert.IsTrue(found[typeof(BasicValidationObject)].Count == 4);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        [ExpectedException(typeof(ArgumentException))]
        public void Validation_FindsCorrectAmountOfValidators()
        {
            var dictionary = new Dictionary<object, object>();
            dictionary.Add("Hello", "Goodbye");

            var attributes = AttributeFinder.Find<ValidationAttribute>(typeof(PipelineStageTestModel));
            Assert.AreEqual(6, attributes[typeof(PipelineStageTestModel)].First().Value.Count);

            attributes = AttributeFinder.Find<ValidationAttribute>(typeof(PipelineStageTestModel), false, false, null);
            Assert.AreEqual(6, attributes[typeof(PipelineStageTestModel)].First().Value.Count);

            attributes = AttributeFinder.Find<ValidationAttribute>(typeof(PipelineStateTestModelDerived), false, false, null);
            Assert.AreEqual(2, attributes[typeof(PipelineStateTestModelDerived)].First().Value.Count);

            attributes = AttributeFinder.Find<ValidationAttribute>(typeof(PipelineStateTestModelDerived), false, false, null);
            Assert.AreEqual(8, attributes[typeof(PipelineStateTestModelDerived)].First().Value.Count);

            var a = (typeof(PipelineStateTestModelDerived)).GetCustomAttributes(false);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Validation_HasErrors_TestCount()
        {
            ValidationSummary summary = new ValidationSummary();
            summary.Violations.Add("Key1", new List<ValidationViolation>() { new ValidationViolation(ValidationSeverity.Error, "error", "string"), new ValidationViolation(ValidationSeverity.Warning, "warning", "string") });
            summary.Violations.Add("Key2", new List<ValidationViolation>() { new ValidationViolation(ValidationSeverity.Error, "error", "string") });

            Assert.AreEqual(summary.Errors.Count, 2);
            Assert.AreEqual(summary.Warnings.Count, 1);

            summary = new ValidationSummary();
            summary.Violations.Add("Key1", new List<ValidationViolation>() { new ValidationViolation(ValidationSeverity.Error, "error", "string"), new ValidationViolation(ValidationSeverity.Error, "Error 2", "string") });
            summary.Violations.Add("Key2", new List<ValidationViolation>() { new ValidationViolation(ValidationSeverity.Error, "error", "string") });

            Assert.AreEqual(summary.Errors.Count, 2);
            Assert.AreEqual(summary.Warnings.Count, 0);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationLoader_AttributeTest_Valid()
        {
            var obj = new DomainObject17();
            obj.ID = 5;
            var r = ValidationHandler.Validate(obj);
            var sum = ValidationSummary.Map(r);

            Assert.IsNull(r, "Validation Object returned not null for model with validation attributes");
            Assert.IsNotNull(sum.Errors.Count == 0);
        }
    }
}