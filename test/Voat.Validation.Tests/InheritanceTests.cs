using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Voat.Validation.Tests
{
    [TestClass]
    public class InheritanceTests
    {
        [TestMethod]
        public void Parent_Child_Validator_No_Error()
        {
            var metalGizmo = new MetalGizmo() { ID = "1", Alloy = "iron" };

            var result = ValidationHandler.Validate(metalGizmo);

            Assert.IsNull(result, "Validation have errors");
        }

        [TestMethod]
        public void Parent_Child_Validators_Error()
        {
            var metalGizmo = new MetalGizmo();
            var expectedErrorCount = 2;

            var result = ValidationHandler.Validate(metalGizmo);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedErrorCount, result.Count());
        }

        [TestMethod]
        public void Parent_Validators_Error()
        {
            var metalGizmo = new MetalGizmo() { Alloy = "iron" };
            var expectedErrorCount = 1;
            var expectedErrorMssg = "ID is empty";

            var result = ValidationHandler.Validate(metalGizmo);

            Assert.IsNotNull(result, "Validation for {0} fail", metalGizmo.ID);
            Assert.AreEqual(expectedErrorCount, result.Count());
            Assert.AreEqual(expectedErrorMssg, result.FirstOrDefault().ErrorMessage);
        }
    }
}