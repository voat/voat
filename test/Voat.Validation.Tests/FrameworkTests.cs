using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Voat.Common.Fs;

namespace Voat.Validation.Tests
{
    [TestClass]
    public class FrameworkTests
    {
        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Attribute_Filtering_All()
        {
            var dictionary = AttributeFinder.Find<ValidationAttribute>(typeof(FilteredObject));
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual(6, dictionary[typeof(FilteredObject)].First().Value.Count);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Attribute_Filtering_All_NullFilter()
        {
            var dictionary = AttributeFinder.Find<ValidationAttribute>(typeof(FilteredObject));
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual(6, dictionary[typeof(FilteredObject)].First().Value.Count);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Attribute_Filtering_DataValidation()
        {
            var dictionary = AttributeFinder.Find<ValidationAttribute>(typeof(FilteredObject), (x) => x is DataValidationAttribute);
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual(4, dictionary[typeof(FilteredObject)].First().Value.Count);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Attribute_Filtering_DataValidation_Pipeline()
        {
            var filter = new Func<Attribute, bool>((x) =>
            {
                var d = x as DataValidationAttribute;
                if (d != null)
                {
                    return d.Pipeline == "Simple";
                }
                return false;
            });

            var dictionary = AttributeFinder.Find<ValidationAttribute>(typeof(FilteredObject), filter);
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual(2, dictionary[typeof(FilteredObject)].First().Value.Count);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void Attribute_Filtering_DataValidation_Pipeline_Stage()
        {
            var filter = new Func<Attribute, bool>((x) =>
            {
                var datavalidationAttribute = x as DataValidationAttribute;
                if (datavalidationAttribute != null)
                {
                    return datavalidationAttribute.Pipeline == "Simple" && datavalidationAttribute.Stage == "BeforeSave";
                }
                return false;
            });

            var dictionary = AttributeFinder.Find<ValidationAttribute>(typeof(FilteredObject), filter);
            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual(1, dictionary[typeof(FilteredObject)].First().Value.Count);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void DataAttribute_Invalid()
        {
            var obj = new UnitTestModel() { Name = "<forbidden phrase>" };
            var result = ValidationHandler.Validate(obj);
            Assert.IsNotNull(result);

            Assert.IsTrue(result.Any(x => x.ErrorMessage.Contains("<forbidden phrase>")));
            Assert.IsTrue(result.Any(x => x.MemberNames.Any(n => n == "Name")));
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void DataAttribute_Valid()
        {
            var obj = new UnitTestModel() { Name = "F#" };
            var result = ValidationHandler.Validate(obj);
            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void DataValidator_UniqueID()
        {
            var obj = new UniqueIDNameValidatorTest("2.1");
            Assert.AreEqual("2.1", obj.RuleID);
            Assert.AreEqual(String.Format("{0}<{1}>:{2}", typeof(UniqueIDNameValidatorTest).Name, "Object", "2.1"), obj.UniqueID);

            obj = new UniqueIDNameValidatorTest(null);
            Assert.AreEqual("", obj.RuleID);
            Assert.AreEqual(String.Format("{0}<{1}>", typeof(UniqueIDNameValidatorTest).Name, "Object"), obj.UniqueID);
        }
        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationPathResult_ErrorMessagePropagation()
        {
            string message = "Test, test, testaroo";
            var result = new ValidationPathResult(message, "Name", "Required");
            Assert.AreEqual(result.ErrorMessage, message);

            result = ValidationPathResult.Create("", message, x => x);
            Assert.AreEqual(result.ErrorMessage, message);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationPathResult_NonConstantDictionaryIndexer()
        {
            int i = 30;
            var result = ValidationPathResult.Create(new NestedDictionaryValidationObject<int>(), "Test, test, testaroo", x => x.NestedDictionary[i].DoubleProperty);
            Assert.AreEqual(String.Format("NestedDictionary[{0}].DoubleProperty", i), result.MemberNames.First());
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationPathResult_NonConstantListIndexer()
        {
            int i = 50;
            var result = ValidationPathResult.Create(new NestedListValidationObject(), "Test, test, testaroo", x => x.NestedObject[i]);
            Assert.AreEqual(String.Format("NestedObject[{0}]", i), result.MemberNames.First());
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationPathResult_NonConstantNestedIndexer()
        {
            int i = 0;
            int i3 = 17;
            var result = ValidationPathResult.Create(new NestedEndlessListValidationObject(), "Test, test, testaroo", x => x.NestedObject[i].NestedObject[i3]);
            Assert.AreEqual(String.Format("NestedObject[{0}].NestedObject[{1}]", i, i3), result.MemberNames.First());
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationPathResult_PathRootIsModel()
        {
            string message = "Test, test, testaroo";
            var result = new ValidationPathResult(message, "Name", "Required");
            Assert.AreEqual(result.ErrorMessage, message);

            result = ValidationPathResult.Create("", message, x => x);
            Assert.AreEqual("", result.MemberNames.First());
        }
    }
}