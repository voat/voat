using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Voat.Common.Fs;

namespace Voat.Validation.Tests
{
    [TestClass]
    public class ValidationLoaderTests
    {
        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidationLoader_AttributeTest_BadTypeName()
        {
            var obj = new DomainObjectError();
            var result = ValidationHandler.Validate(obj);
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationLoader_AttributeTest_Invalid()
        {
            var obj = new DomainObject17();
            obj.ID = -5;
            var result = ValidationHandler.Validate(obj);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FirstOrDefault(x => x.MemberNames.Any(y => y == "ID")));
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationLoader_Loads_Base_Validators_Via_Loader()
        {
            var result = AttributeFinder.Find<ValidationAttribute>(typeof(ValidationLoaderBase));
            Assert.AreEqual(1, result[typeof(ValidationLoaderBase)].First().Value.Count());
        }

        [TestMethod]
        [TestCategory("Validation"), TestCategory("Validation.Framework")]
        public void ValidationLoader_Loads_Derived_Validators_Via_Loader()
        {
            var result = AttributeFinder.Find<ValidationAttribute>(typeof(ValidationLoaderDerived));
            Assert.AreEqual(2, result[typeof(ValidationLoaderDerived)].First().Value.Count());
        }
    }

    #region Objects

    [Loader("Voat.Validation.Tests.ValidationLoaderClass, Voat.Validation.Tests")]
    public class DomainObject17
    {
        public int ID { get; set; }
    }

    [Loader("Voat.Validation.Tests.CantBeFound, Voat.Validation.Tests")]
    public class DomainObjectError
    {
        public int ID { get; set; }
    }

    [Loader("Voat.Validation.Tests.ValidationLoaderBaseLoader, Voat.Validation.Tests")]
    public class ValidationLoaderBase
    {
    }

    [DataValidation(typeof(ValidateID))]
    public class ValidationLoaderBaseLoader
    {
    }

    [DataValidation(typeof(ValidateID))]
    public class ValidationLoaderClass
    {
    }
    [Loader("Voat.Validation.Tests.ValidationLoaderDerivedLoader, Voat.Validation.Tests")]
    public class ValidationLoaderDerived : ValidationLoaderBase
    {
    }

    [DataValidation(typeof(ValidateID))]
    public class ValidationLoaderDerivedLoader
    {
    }
    #endregion Objects
}