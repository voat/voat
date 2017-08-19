using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Voat.Validation.Tests
{
    //TODO: Test context, will need in the future, right now this isn't supported but ms runtime uses this so we need to support it.
    [TestClass]
    public class ContextTests
    {
        [TestMethod]
        [TestCategory("Validation")]
        [TestCategory("Validation.Framework")]
        public void Ensure_Context_Handles_Nulls()
        {
            var testContext = new TestContext();
            testContext.KeyName = "Name here";
            testContext.RunTimeType = typeof(string);
            testContext.Value = "Value here";
            var result = ValidationHandler.Validate(testContext, null);
        }

        [TestMethod]
        [TestCategory("Validation")]
        [TestCategory("Validation.Framework")]
        public void Ensure_Context_Is_Persisted()
        {
            var context = Tuple.Create("Name here", "Value here");

            var contextDictionary = new Dictionary<object, object>();
            contextDictionary.Add(context.Item1, context.Item2);

            var testContext = new TestContext();
            testContext.KeyName = context.Item1;
            testContext.RunTimeType = typeof(string);
            testContext.Value = context.Item2;

            var result = ValidationHandler.Validate(testContext, contextDictionary);
            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("Validation")]
        [TestCategory("Validation.Framework")]
        public void Ensure_Context_Is_Validated()
        {
            var context = Tuple.Create("Name here", "Value here");
            var contextDictionary = new Dictionary<object, object>();
            contextDictionary.Add(context.Item1, context.Item2);

            var testContext = new TestContext();
            testContext.KeyName = context.Item1;
            testContext.RunTimeType = typeof(string);
            testContext.Value = "Different Value";

            var result = ValidationHandler.Validate(testContext, contextDictionary);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            testContext.RunTimeType = typeof(object);
            testContext.Value = context.Item2; //set back to original
            result = ValidationHandler.Validate(testContext, contextDictionary);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            contextDictionary.Remove(context.Item1);
            result = ValidationHandler.Validate(testContext, contextDictionary);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [DataValidation(typeof(TestContextValidator))]
        public class TestContext
        {
            public string KeyName { get; set; }
            public Type RunTimeType { get; set; }
            public object Value { get; set; }
        }

        public class TestContextValidator : DataValidator<TestContext>
        {
            public override IEnumerable<ValidationPathResult> Validate(TestContext value, ValidationContext context)
            {
                var validationViolations = new List<ValidationPathResult>();

                if (context != null)
                {
                    var o = context.Items.ContainsKey(value.KeyName) ? context.Items[value.KeyName] : null;
                    if (o == null)
                    {
                        validationViolations.Add(new ValidationPathResult(String.Format("Key {0} not present in dictionary", value.KeyName), "", "Error"));
                    }
                    else
                    {
                        if (o.GetType() != value.RunTimeType)
                        {
                            validationViolations.Add(new ValidationPathResult(String.Format("Key {0} is typed as {1} but was excpected to be {2}", value.KeyName, o.GetType().Name, value.RunTimeType.Name), "", "Error"));
                        }
                        if (value.Value != null && o != value.Value)
                        {
                            validationViolations.Add(new ValidationPathResult(String.Format("Key {0} expected value of {1} but was {2}", value.KeyName, value.Value.ToString(), o.ToString()), "", "Error"));
                        }
                    }
                }
                return validationViolations.Count == 0 ? null : validationViolations;
            }
        }
    }
}