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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Base.NUnit
{

    [TestClass]
    public partial class UnitTestTestClass2 : BaseUnitTest
    {
        private static int testInitCount = 0;
        private static int classInitCount = 0;
        private int preInitCount = 0;
        public override void TestInitialize()
        {
            preInitCount = testInitCount;
            testInitCount += 1;
        }

        public override void ClassInitialize()
        {
            classInitCount += 1;
        }

        [TestMethod]
        [TestCategory("UnitTest.Framework")]
        public void Method1_2()
        {
            Assert.AreEqual(1, classInitCount);
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
        }

        [TestMethod]
        [TestCategory("UnitTest.Framework")]
        public void Method2_2()
        {
            Assert.AreEqual(1, classInitCount);
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
        }

        [TestMethod]
        [TestCategory("UnitTest.Framework")]
        public void Method3_2()
        {
            Assert.AreEqual(1, classInitCount);
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
        }
    }
    [TestClass]
    public partial class UnitTestTestClass1 : BaseUnitTest
    {
        private static int testInitCount = 0;
        private static int classInitCount = 0;
        private int preInitCount = 0;
        public override void TestInitialize()
        {
            preInitCount = testInitCount;
            testInitCount += 1;
        }

        public override void ClassInitialize()
        {
            classInitCount += 1;
        }
        
        [TestMethod]
        [TestCategory("UnitTest.Framework")]
        public void Method1()
        {
            Assert.AreEqual(1, classInitCount);
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
        }

        [TestMethod]
        [TestCategory("UnitTest.Framework")]
        public void Method2()
        {
            Assert.AreEqual(1, classInitCount);
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
        }

        [TestMethod]
        [TestCategory("UnitTest.Framework")]
        public void Method3()
        {
            Assert.AreEqual(1, classInitCount);
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
        }
    }
}
