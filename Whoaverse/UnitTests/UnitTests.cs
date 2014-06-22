/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whoaverse.Utils;

namespace UnitTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestCalcSubmissionAge()
        {
            DateTime testDate = DateTime.Now.AddDays(-2);
            string result = Submissions.CalcSubmissionAge(testDate);
            Assert.AreEqual("2 days", result, "Submission age was not calculated.");
        }

        [TestMethod]
        public void TestCalcSubmissionAgeDouble()
        {
            DateTime testDate = DateTime.Now.AddDays(-143);
            double result = Submissions.CalcSubmissionAgeDouble(testDate);
            Assert.AreEqual(3432, result, 0.1, "Submission double age was not calculated.");
        }

        [TestMethod]
        public void TestGetDomainFromUri()
        {
            Uri testUri = new Uri("http://www.youtube.com");

            string result = UrlUtility.GetDomainFromUri(testUri.ToString());
            Assert.AreEqual("youtube.com", result, "Unable to extract domain from given Uri.");
        }

        [TestMethod]
        public void TestFormatMarkdown()
        {
            string testString = "**Bold**";

            string result = Formatting.FormatMessage(testString);
            Assert.AreEqual("<p><strong>Bold</strong></p>", result.Trim(), "Markdown formatting failed");
        }

        [TestMethod]
        public void TestCalcRank()
        {            
            double result = Ranking.CalculateNewRank(0.5, 150, 20);
            Assert.AreEqual(0.0555555555555556, result, 0.01, "Rank was not calculated.");
        }
    }
}
