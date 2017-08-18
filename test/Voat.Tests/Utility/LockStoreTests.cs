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
using Voat.Common;
using Voat.Tests.Infrastructure;

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
