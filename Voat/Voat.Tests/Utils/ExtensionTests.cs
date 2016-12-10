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
    public class ExtensionTests : BaseUnitTest
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


        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_String_1()
        {
            Assert.AreEqual(false, "".IsDefault());
        }


        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_String_2()
        {
            Assert.AreEqual(true, ((string)null).IsDefault());
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_Int_1()
        {
            Assert.AreEqual(false, 1.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_Int_2()
        {
            Assert.AreEqual(true, 0.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_Enum_1()
        {
            Assert.AreEqual(false, TestEnum.Value1.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_Enum_2()
        {
            Assert.AreEqual(true, TestEnum.Value0.IsDefault());
        }
        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_Object_1()
        {
            TestObject t = new TestObject();
            Assert.AreEqual(false, t.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void IsDefault_Object_2()
        {
            TestObject t = null;
            Assert.AreEqual(true, t.IsDefault());
        }



        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Convert_Object_1()
        {
            TestObjectParent t = new TestObjectParent();
            TestObject x = t.Convert<TestObject, TestObjectParent>();
            Assert.IsNotNull(x);
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        [ExpectedException(typeof(InvalidCastException))]
        public void Convert_Object_2()
        {
            TestObject t = new TestObject();
            TestObjectParent x = t.Convert<TestObjectParent, TestObject>();
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Convert_int_1()
        {
            object input = (int)17;
            int output = input.Convert<int, object>();
            Assert.AreEqual(17, output);
        }

        [TestMethod]
        [TestCategory("Utilities"), TestCategory("Extentions")]
        public void Convert_Complex_1()
        {
            var input = (object)new HashSet<int>();
            ISet<int> output = input.Convert<ISet<int>, object>();
            Assert.IsNotNull(output);
        }

    }
    public class TestObject
    {

    }
    public class TestObjectParent : TestObject
    {

    }
    public enum TestEnum
    {
        Value0,
        Value1,
        Value2
    }
}
