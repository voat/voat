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
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Null_IsEqual_1()
        {
            string before = null;
            bool result = before.IsEqual(null);
            Assert.AreEqual(true, result);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Null_IsEqual_2()
        {
            string before = null;
            bool result = before.IsEqual("");
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void CaseDifference_IsEqual()
        {
            string before = "lower";
            bool result = before.IsEqual("LOWER");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void ContentDifference_IsEqual()
        {
            string before = "lower";
            bool result = before.IsEqual("UPPER");
            Assert.AreEqual(false, result);
        }


        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Null_TrimSafe()
        {
            string before = null;
            string expected = null;
            string result = before.TrimSafe();
            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Empty_TrimSafe()
        {
            string before = "";
            string expected = "";
            string result = before.TrimSafe();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Padded_TrimSafe()
        {
            string before = " x ";
            string expected = "x";
            string result = before.TrimSafe();
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void HasInterface_Simple()
        {
            var result = typeof(List<object>).HasInterface(typeof(IList));
            Assert.AreEqual(true, result);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void HasInterface_Simple_False()
        {
            var result = typeof(string).HasInterface(typeof(IList));
            Assert.AreEqual(false, result);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void HasInterface_Generic_Generic_Partial()
        {
            var result = typeof(HashSet<object>).HasInterface(typeof(ISet<>));
            Assert.AreEqual(true, result);
            
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void HasInterface_Generic_Generic_FullyQualified()
        {
            var result = typeof(HashSet<object>).HasInterface(typeof(ISet<object>));
            Assert.AreEqual(true, result);
        }


        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_String_1()
        {
            Assert.AreEqual(false, "".IsDefault());
        }


        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_String_2()
        {
            Assert.AreEqual(true, ((string)null).IsDefault());
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_Int_1()
        {
            Assert.AreEqual(false, 1.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_Int_2()
        {
            Assert.AreEqual(true, 0.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_Enum_1()
        {
            Assert.AreEqual(false, TestEnum.Value1.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_Enum_2()
        {
            Assert.AreEqual(true, TestEnum.Value0.IsDefault());
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_Object_1()
        {
            TestObject t = new TestObject();
            Assert.AreEqual(false, t.IsDefault());
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void IsDefault_Object_2()
        {
            TestObject t = null;
            Assert.AreEqual(true, t.IsDefault());
        }



        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Convert_Object_1()
        {
            TestObjectParent t = new TestObjectParent();
            TestObject x = t.Convert<TestObject, TestObjectParent>();
            Assert.IsNotNull(x);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        [ExpectedException(typeof(InvalidCastException))]
        public void Convert_Object_2()
        {
            TestObject t = new TestObject();
            TestObjectParent x = t.Convert<TestObjectParent, TestObject>();
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Convert_int_1()
        {
            object input = (int)17;
            int output = input.Convert<int, object>();
            Assert.AreEqual(17, output);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void Convert_Complex_1()
        {
            var input = (object)new HashSet<int>();
            ISet<int> output = input.Convert<ISet<int>, object>();
            Assert.IsNotNull(output);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void ToQueryString()
        {

            Assert.AreEqual("text=Hello%2C%20I%27m%20writing%20%23text%20in%20a%20url%21%3F", (new { text = "Hello, I'm writing #text in a url!?" }).ToQueryString());
            Assert.AreEqual("text=http%3A%2F%2Fwww.voat.co%2Fv%2Fall%2Fnew%3Fpage%3D3%26show%3Dall", (new { text = "http://www.voat.co/v/all/new?page=3&show=all" }).ToQueryString());

            Assert.AreEqual("id=4", (new { id = 4 }).ToQueryString());
            Assert.AreEqual("id=1&id2=2&id3=3&id4=four", (new { id = 1, id2 = 2, id3 = "3", id4 = "four" }).ToQueryString());
            Assert.AreEqual("id=4&name=joe", (new { id = 4, name = "joe" }).ToQueryString());

            Assert.AreEqual("id=4", (new { id = 4, name = (string)null }).ToQueryString());
            Assert.AreEqual("id=4&name=", (new { id = 4, name = (string)null }).ToQueryString(true));

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
