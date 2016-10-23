using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class ExtensionTests
    {

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Null_IsEqual_1()
        {
            string before = null;
            bool result = before.IsEqual(null);
            Assert.AreEqual(true, result);
        }
        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Null_IsEqual_2()
        {
            string before = null;
            bool result = before.IsEqual("");
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void CaseDifference_IsEqual()
        {
            string before = "lower";
            bool result = before.IsEqual("LOWER");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void ContentDifference_IsEqual()
        {
            string before = "lower";
            bool result = before.IsEqual("UPPER");
            Assert.AreEqual(false, result);
        }


        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Null_TrimSafe()
        {
            string before = null;
            string expected = null;
            string result = before.TrimSafe();
            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Empty_TrimSafe()
        {
            string before = "";
            string expected = "";
            string result = before.TrimSafe();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Padded_TrimSafe()
        {
            string before = " x ";
            string expected = "x";
            string result = before.TrimSafe();
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void HasInterface_Simple()
        {
            var result = typeof(List<object>).HasInterface(typeof(IList));
            Assert.AreEqual(true, result);
        }
        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void HasInterface_Simple_False()
        {
            var result = typeof(string).HasInterface(typeof(IList));
            Assert.AreEqual(false, result);
        }
        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void HasInterface_Generic_Generic_Partial()
        {
            var result = typeof(HashSet<object>).HasInterface(typeof(ISet<>));
            Assert.AreEqual(true, result);
            
        }
        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void HasInterface_Generic_Generic_FullyQualified()
        {
            var result = typeof(HashSet<object>).HasInterface(typeof(ISet<object>));
            Assert.AreEqual(true, result);
        }
    }
}
