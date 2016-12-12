using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class LockStoreTests : BaseUnitTest
    {

        [TestMethod]
        public void TestLock()
        {
            var lockStore = new LockStore();
            var o1 = lockStore.GetLockObject("MyString");
            var o2 = lockStore.GetLockObject("MyString2");
            Assert.AreNotEqual(o1, o2, "Should not have same lock object");
            var o1_2 = lockStore.GetLockObject("MyString");
            Assert.AreEqual(o1, o1_2, "Should have same lock object");
        }
        [TestMethod]
        public void TestLockSemaphoreSlim()
        {
            var lockStore = new SemaphoreSlimLockStore();
            var o1 = lockStore.GetLockObject("MyString");
            var o2 = lockStore.GetLockObject("MyString2");
            Assert.AreNotEqual(o1, o2, "Should not have same lock object");
            var o1_2 = lockStore.GetLockObject("MyString");
            Assert.AreEqual(o1, o1_2, "Should have same lock object");
        }

        [TestMethod]
        public void TestLockNewLocks()
        {
            var lockStore = new LockStore(false);
            var o1 = lockStore.GetLockObject("MyString");
            var o2 = lockStore.GetLockObject("MyString2");
            Assert.AreNotEqual(o1, o2, "Should not have same lock object");
            var o1_2 = lockStore.GetLockObject("MyString");
            Assert.AreNotEqual(o1, o1_2, "Should have same lock object");
        }
    }
}
