using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Tests.Base.NUnit
{
    [TestClass]
    public class UnitTestTestClass : BaseUnitTest
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
        public void Method1()
        {
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
            Assert.AreEqual(1, classInitCount);
        }

        [TestMethod]
        public void Method2()
        {
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
            Assert.AreEqual(1, classInitCount);
        }

        [TestMethod]
        public void Method3()
        {
            Assert.AreEqual(this.preInitCount + 1, testInitCount);
            Assert.AreEqual(1, classInitCount);
        }
    }
}
