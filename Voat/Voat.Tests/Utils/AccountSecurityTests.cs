/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System.Diagnostics.PerformanceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Business.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class AccountSecurityTests
    {
        [TestMethod]
        public void TestIsPasswordComplex()
        {
            const string testPassword1 = "donthackmeplox";
            const string testPassword2 = "tHisIs4P4ssw0rd!";

            const string testUsername1 = "donthackmeplox";
            const string testUsername2 = "TheSpoon";

            bool result1 = AccountSecurity.IsPasswordComplex(testPassword1, testUsername1);
            Assert.AreEqual(false, result1, "Unable to check password complexity for insecure password.");

            bool result2 = AccountSecurity.IsPasswordComplex(testPassword2, testUsername2);
            Assert.AreEqual(true, result2, "Unable to check password complexity for secure password.");

            bool result3 = AccountSecurity.IsPasswordComplex(testPassword1, testUsername2);
            Assert.AreEqual(false, result3, "Unable to check password complexity for insecure password.");
        }
    }
}
