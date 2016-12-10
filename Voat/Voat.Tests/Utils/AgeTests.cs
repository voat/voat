#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Diagnostics;
using Voat.Common;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class AgeTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Bug_Trap_0_hour_bug()
        {
            ////I can't trap this bug but I know it occurs. I've seen it. Don't tell me I'm crazy! (Maybe it's old code. I've written this stuff like 4 times now.)
            var timespan = TimeSpan.FromTicks(0);

            while (timespan <= TimeSpan.FromHours(3))
            {
                string result = Age.ToRelative(timespan);
                Debug.Print(result);
                if (result.StartsWith("0"))
                {
                    Assert.Fail(String.Format("{0} ticks breaks this mofo!", timespan.Ticks));
                }
                timespan = timespan + TimeSpan.FromTicks(499923); //add less than half a second
            }
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Bug_Trap_0_second_bug()
        {
            var timespan = TimeSpan.FromTicks(999);
            string result = Age.ToRelative(timespan);
            Assert.AreEqual("1 second", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Bug_Trap_Negative_Spans()
        {
            //this occurs if time stamps on servers get out of line
            string result = Age.ToRelative(TimeSpan.FromHours(-0.98));
            Assert.AreEqual("sometime", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1_21second()
        {
            string result = Age.ToRelative(TimeSpan.FromSeconds(1.2096));
            Assert.AreEqual("1 microfortnight", result, "Submission age was not calculated.");

            result = Age.ToRelative(TimeSpan.FromSeconds(1.212));
            Assert.AreEqual("1 microfortnight", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1_5years()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(18 * 30));
            Assert.AreEqual(String.Format("{0} years", 1.5), result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1_7Days()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(1.7));
            Assert.AreEqual("1.7 days", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1Day()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(1));
            Assert.AreEqual("1 day", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1minute()
        {
            string result = Age.ToRelative(TimeSpan.FromMinutes(1));
            Assert.AreEqual("1 minute", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1point2hours()
        {
            string result = Age.ToRelative(TimeSpan.FromHours(1.22323232));
            Assert.AreEqual("1.2 hours", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1second()
        {
            string result = Age.ToRelative(TimeSpan.FromSeconds(1));
            Assert.AreEqual("1 second", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_1year()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(370));
            Assert.AreEqual("1 year", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_2Day()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(2));
            Assert.AreEqual("2 days", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_2minutes()
        {
            string result = Age.ToRelative(TimeSpan.FromMinutes(2));
            Assert.AreEqual("2 minutes", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_2years()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(369 * 2));
            Assert.AreEqual("2 years", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_3_1Days()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(3.2));
            Assert.AreEqual("3 days", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_30seconds()
        {
            string result = Age.ToRelative(TimeSpan.FromSeconds(30));
            Assert.AreEqual("30 seconds", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_52Day()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(52));
            Assert.AreEqual("1 dog year", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_52minutes()
        {
            string result = Age.ToRelative(TimeSpan.FromMinutes(52));
            Assert.AreEqual("1 microcentury", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_8months()
        {
            string result = Age.ToRelative(TimeSpan.FromDays(8 * 31));
            Assert.AreEqual("8 months", result, "Submission age was not calculated.");
        }

        [TestMethod]
        [TestCategory("Age")]
        [TestCategory("Calculation")]
        [TestCategory("Utility")]
        public void Age_90minutes()
        {
            string result = Age.ToRelative(TimeSpan.FromMinutes(90));
            Assert.AreEqual(String.Format("{0} hours", 1.5), result, "Submission age was not calculated.");
        }

    }
}
