using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Voat.Common.Fs;
using Voat.Validation.Tests.TestObjects;

namespace Voat.Validation.Tests
{
    [TestClass]
    public class PathingTests
    {
        [TestMethod]
        public void Pathing_Tests_IValidatableObject()
        {
            Nested<NestedSubType> n;
            n = new Nested<NestedSubType>();
            n.Options = new List<NestedSubType> {
                new NestedSubType() { ReturnValidationError = false },
                new NestedSubType() { ReturnValidationError = false }
            };

            var result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            Assert.IsNull(result);


            n = new Nested<NestedSubType>();
            n.Options = new List<NestedSubType> {
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true },
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true }
            };

            result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            Assert.IsNotNull(result);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual($"Options[{i}].UserName", result[i].MemberNames.FirstOrDefault());
            }

            n = new Nested<NestedSubType>();
            n.Options = new List<NestedSubType> {
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = false },
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = false }
            };

            result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            Assert.IsNotNull(result);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual($"Options[{i}].UserName", result[i].MemberNames.FirstOrDefault());
            }

            n = new Nested<NestedSubType>();
            n.Options = new List<NestedSubType> {
                new NestedSubType() { ReturnValidationError = false},
            };
            n.Options[0].EndlessNesting.Add(new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true });
            n.Options[0].EndlessNesting.Add(new NestedSubType() { ReturnValidationError = false, UseValidationPathResult = true });
            n.Options[0].EndlessNesting.Add(new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true });

            result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual($"Options[0].EndlessNesting[0].UserName", result[0].MemberNames.FirstOrDefault());
            Assert.AreEqual($"Options[0].EndlessNesting[2].UserName", result[1].MemberNames.FirstOrDefault());

        }
        [TestMethod]
        public void Pathing_Tests_IValidatableObject_BaseType()
        {
            Nested<NestedSubTypeBase> n;

            //var t = AttributeFinder.FindValidatableProperties(typeof(NestedSubType));
            //t.First().Item1.GetCustomAttributes(

            n = new Nested<NestedSubTypeBase>();
            n.Options = new List<NestedSubTypeBase> {
                new NestedSubType() { ReturnValidationError = false },
                new NestedSubType() { ReturnValidationError = false }
            };

            var result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            Assert.IsNull(result);


            n = new Nested<NestedSubTypeBase>();
            n.Options = new List<NestedSubTypeBase> {
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true },
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true }
            };

            result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            Assert.IsNotNull(result);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual($"Options[{i}].UserName", result[i].MemberNames.FirstOrDefault());
            }

            n = new Nested<NestedSubTypeBase>();
            n.Options = new List<NestedSubTypeBase> {
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = false },
                new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = false }
            };

            result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            Assert.IsNotNull(result);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual($"Options[{i}].UserName", result[i].MemberNames.FirstOrDefault());
            }

            //n = new Nested<NestedSubTypeBase>();
            //n.Options = new List<NestedSubTypeBase> {
            //    new NestedSubType() { ReturnValidationError = false},
            //};
            //n.Options[0].EndlessNesting.Add(new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true });
            //n.Options[0].EndlessNesting.Add(new NestedSubType() { ReturnValidationError = false, UseValidationPathResult = true });
            //n.Options[0].EndlessNesting.Add(new NestedSubType() { ReturnValidationError = true, UseValidationPathResult = true });

            //result = ValidationHandler.Validate(n, new Dictionary<object, object>(), false);
            //Assert.IsNotNull(result);
            //Assert.AreEqual(2, result.Count);
            //Assert.AreEqual($"Options[0].EndlessNesting[0].UserName", result[0].MemberNames.FirstOrDefault());
            //Assert.AreEqual($"Options[0].EndlessNesting[2].UserName", result[1].MemberNames.FirstOrDefault());

        }
    }
}
