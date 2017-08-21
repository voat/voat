#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Models;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class ExtensionTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void TestReverseSplit()
        {
            Assert.AreEqual("co.voat", "voat.co".ReverseSplit("."));
            Assert.AreEqual("co.voat.api", "api.voat.co".ReverseSplit("."));
            Assert.AreEqual("jason", "jason".ReverseSplit("."));
            Assert.AreEqual("", "".ReverseSplit("."));
            Assert.AreEqual(null, ((string)null).ReverseSplit("."));

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void TestTrimSafe()
        {
            Assert.AreEqual(null, ((string)null).TrimSafe());
            Assert.AreEqual("", "".TrimSafe());
            Assert.AreEqual("", " ".TrimSafe());
            Assert.AreEqual(".", " . ".TrimSafe());

            Assert.AreEqual("jpg", ".jpg".TrimSafe("."));
            Assert.AreEqual("jpg", ".jpg.".TrimSafe("."));

        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void TestRelativePathingTests()
        {
            var parts = "somefile.txt".ToPathParts().ToList();
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("somefile.txt", parts[0]);

            parts = "~/somefile.txt".ToPathParts().ToList();
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("somefile.txt", parts[0]);

            parts = "~/folder/somefile.txt".ToPathParts().ToList();
            Assert.AreEqual(2, parts.Count);
            Assert.AreEqual("folder", parts[0]);
            Assert.AreEqual("somefile.txt", parts[1]);

            parts = "\\folder\\somefile.txt".ToPathParts().ToList();
            Assert.AreEqual(2, parts.Count);
            Assert.AreEqual("folder", parts[0]);
            Assert.AreEqual("somefile.txt", parts[1]);

            parts = "..\\folder\\somefile.txt".ToPathParts().ToList();
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual("..", parts[0]);
            Assert.AreEqual("folder", parts[1]);
            Assert.AreEqual("somefile.txt", parts[2]);

            parts = new string[] { "~/one/two", "..\\folder\\somefile.txt" }.ToPathParts().ToList();
            Assert.AreEqual(5, parts.Count);
            Assert.AreEqual("one", parts[0]);
            Assert.AreEqual("two", parts[1]);
            Assert.AreEqual("..", parts[2]);
            Assert.AreEqual("folder", parts[3]);
            Assert.AreEqual("somefile.txt", parts[4]);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void EnsureRangeTests()
        {
            Assert.AreEqual(10, 10.EnsureRange(0, 20));
            Assert.AreEqual(10, 10.EnsureRange(10, 20));
            Assert.AreEqual(15, 10.EnsureRange(15, 20));
            Assert.AreEqual(5, 10.EnsureRange(0, 5));
            Assert.AreEqual(-10, 10.EnsureRange(-20, -10));

            Assert.AreEqual(1.1, 0.9.EnsureRange(1.1, 1.9));
            Assert.AreEqual(1.2, 1.2.EnsureRange(1.1, 1.9));
            Assert.AreEqual(1.9, 2.7.EnsureRange(1.1, 1.9));

        }

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
        //[ExpectedException(typeof(InvalidCastException))]
        public void Convert_Object_2()
        {
            VoatAssert.Throws<InvalidCastException>(() => {
                TestObject t = new TestObject();
                TestObjectParent x = t.Convert<TestObjectParent, TestObject>();
            });
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

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void TestEnumParsing()
        {



            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Top, Voat.Common.Extensions.AssignIfValidEnumValue(-23223, Domain.Models.CommentSortAlgorithm.Top));
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Top, Voat.Common.Extensions.AssignIfValidEnumValue(324324, Domain.Models.CommentSortAlgorithm.Top));
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.New, Voat.Common.Extensions.AssignIfValidEnumValue((int)Domain.Models.CommentSortAlgorithm.New, Domain.Models.CommentSortAlgorithm.Top));
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Top, Voat.Common.Extensions.AssignIfValidEnumValue(null, Domain.Models.CommentSortAlgorithm.Top));
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Intensity, Voat.Common.Extensions.AssignIfValidEnumValue((int)Domain.Models.CommentSortAlgorithm.Intensity, Domain.Models.CommentSortAlgorithm.Top));

            Assert.IsFalse(Voat.Common.Extensions.IsValidEnumValue((Domain.Models.CommentSortAlgorithm?)777));
            Assert.IsFalse(Voat.Common.Extensions.IsValidEnumValue((Domain.Models.CommentSortAlgorithm?)null));
            Assert.IsTrue(Voat.Common.Extensions.IsValidEnumValue((Domain.Models.CommentSortAlgorithm?)2));

            Assert.IsFalse(Voat.Common.Extensions.IsValidEnumValue((Domain.Models.CommentSortAlgorithm)777));
            Assert.IsTrue(Voat.Common.Extensions.IsValidEnumValue((Domain.Models.CommentSortAlgorithm)2));

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        public void TestSafeEnum_Conversions()
        {
            var safeClass = new TestEnumClass();
            safeClass.CommentSort = Domain.Models.CommentSortAlgorithm.Top;

            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Top,  safeClass.CommentSort.Value);
            Assert.IsTrue(Domain.Models.CommentSortAlgorithm.Top == safeClass.CommentSort);
            Assert.IsTrue(safeClass.CommentSort == Domain.Models.CommentSortAlgorithm.Top);

            Domain.Models.CommentSortAlgorithm x = Domain.Models.CommentSortAlgorithm.New;
            x = safeClass.CommentSort;
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Top, x);

            switch (safeClass.CommentSort.Value)
            {
                case Domain.Models.CommentSortAlgorithm.Top:
                    break;
                default:
                    Assert.Fail("This is a problem");
                    break;
            }

            safeClass.CommentSort = 4;
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Bottom, safeClass.CommentSort.Value);

            safeClass.CommentSort = "New";
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.New, safeClass.CommentSort.Value);

            safeClass.CommentSort = "5";
            Assert.AreEqual(Domain.Models.CommentSortAlgorithm.Intensity, safeClass.CommentSort.Value);


        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        //[ExpectedException(typeof(TypeInitializationException))]
        public void TestSafeEnum_ConstructionError()
        {
            VoatAssert.Throws<TypeInitializationException>(() => {
                var s = new SomeStruct();
                SafeEnum<SomeStruct> x = new SafeEnum<SomeStruct>(s);
            });

        }


        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        //[ExpectedException(typeof(TypeInitializationException))]
        public void TestSafeEnum_ConstructionError2()
        {
            VoatAssert.Throws<TypeInitializationException>(() => {
                SafeEnum<int> x = new SafeEnum<int>(45);
            });

        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSafeEnum_Errors()
        {
            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                var safeClass = new TestEnumClass();
                safeClass.CommentSort = ((Domain.Models.CommentSortAlgorithm)(-203));
            });

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSafeEnum_Errors_2()
        {
            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                var safeClass = new TestEnumClass();
                safeClass.CommentSort = ((Domain.Models.CommentSortAlgorithm)(203));
            });

        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSafeEnum_Errors_3()
        {
            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                var safeClass = new TestEnumClass();
                safeClass.CommentSort = 203;
            });

        }
        [Flags]
        public enum Numbers
        {
            One = 1,
            Two = 2,
            Three = One | Two,
            Four = 4,
            Five = Four | One,
            Six = Four | Two,
            Seven = Four | Two | One,
            Eight = 8,
            Nine = Eight | One,
            Ten = Eight | Two
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Extentions")]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Enum_Flag_Tests()
        {
            var check = new Action<IEnumerable<Numbers>, IEnumerable<Numbers>>((first, second) =>
            {
                Assert.AreEqual(first.Count(), second.Count());
                first.ToList().ForEach(x => {
                    Assert.IsTrue(second.Contains(x));
                });
            });

            var val = Numbers.Ten;
            var values = val.GetEnumFlags();
            check(values, new[] { Numbers.Eight, Numbers.Two });

            val = Numbers.Eight;
            values = val.GetEnumFlags();
            check(values, new[] { Numbers.Eight });

            val = Numbers.Seven;
            values = val.GetEnumFlags();
            check(values, new[] { Numbers.Four, Numbers.Two, Numbers.One });

            values = val.GetEnumFlagsIntersect(Numbers.Six);
            check(values, new[] { Numbers.Four, Numbers.Two});


        }
    }

    public class TestEnumClass
    {
        public SafeEnum<Domain.Models.CommentSortAlgorithm> CommentSort { get; set; }
    }

    public class TestObject
    {

    }
    public class TestObjectParent : TestObject
    {

    }
    public struct SomeStruct : IConvertible
    {
        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }
    public enum TestEnum
    {
        Value0,
        Value1,
        Value2
    }
}
