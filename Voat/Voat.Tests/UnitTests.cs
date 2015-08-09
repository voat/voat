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

using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenGraph_Net;
using Voat.Utilities;

namespace UnitTests
{
    [TestClass]
    public class SubmissionAgeTests
    {

        [TestMethod]
        public void Calc_SubmissionAge_1second()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromSeconds(1));
            Assert.AreEqual("1 second", result, "Submission age was not calculated.");
        }
        [TestMethod]
        public void Calc_SubmissionAge_30seconds()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromSeconds(30));
            Assert.AreEqual("30 seconds", result, "Submission age was not calculated.");
        }
        [TestMethod]
        public void Calc_SubmissionAge_1minute()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromMinutes(1));
            Assert.AreEqual("1 minute", result, "Submission age was not calculated.");
        }
        [TestMethod]
        public void Calc_SubmissionAge_2minutes()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromMinutes(2));
            Assert.AreEqual("2 minutes", result, "Submission age was not calculated.");
        }
        [TestMethod]
        public void Calc_SubmissionAge_1Day()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(1));
            Assert.AreEqual("1 day", result, "Submission age was not calculated.");
        }
        [TestMethod]
        public void Calc_SubmissionAge_2Day()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(2));
            Assert.AreEqual("2 days", result, "Submission age was not calculated.");
        }

        [TestMethod]
        public void Calc_SubmissionAge_8months()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(8 * 31));
            Assert.AreEqual("8 months", result, "Submission age was not calculated.");
        }
        [TestMethod]
        public void Calc_SubmissionAge_1year()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(370));
            Assert.AreEqual("1 year", result, "Submission age was not calculated.");
        }
        [TestMethod]
        public void Calc_SubmissionAge_1_5years()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(18 * 30));
            Assert.AreEqual("1.5 years", result, "Submission age was not calculated.");
        }

        [TestMethod]
        public void Calc_SubmissionAge_2years()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(369 * 2));
            Assert.AreEqual("2 years", result, "Submission age was not calculated.");
        }

        [TestMethod]
        public void Calc_SubmissionAgeDouble()
        {
            DateTime testDate = DateTime.Now.AddDays(-143);
            double result = Submissions.CalcSubmissionAgeDouble(testDate);
            Assert.AreEqual(3432, result, 0.1, "Submission double age was not calculated.");
        }

    }

    [TestClass]
    public class Misc_Tests
    {
        [TestMethod]
        public void TestGetOpenGraphImageFromUri()
        {
            Uri testUri = new Uri("http://www.bbc.com/news/technology-32194196");
            var graph = OpenGraph.ParseUrl(testUri);
            Assert.AreEqual("http://ichef.bbci.co.uk/news/1024/media/images/80755000/jpg/_80755021_163765270.jpg", graph.Image.ToString(), "Unable to extract domain from given Uri.");
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
            Assert.AreEqual(0.0012465, result, 0.01, "Rank was not calculated.");
        }

        [TestMethod]
        public void TestUnicodeDetection()
        {
            const string testString = "🆆🅰🆂 🅶🅴🆃🆃🅸🅽🅶 🅲🅰🆄🅶🅷🆃 🅿🅰🆁🆃 🅾🅵 🆈🅾🆄🆁 🅿🅻🅰🅽🅴";
            const string testStringWithoutUnicode = "was getting caught part of your plane";

            bool result = Submissions.ContainsUnicode(testString);
            Assert.IsTrue(result, "Unicode was not detected.");

            bool resultWithoutUnicode = Submissions.ContainsUnicode(testStringWithoutUnicode);
            Assert.IsFalse(resultWithoutUnicode, "Unicode was not detected.");
        }

        [TestMethod]
        public void TestUnicodeStripping()
        {
            const string testString = "NSA holds info over US citizens like loaded gun, but says ‘trust me’ – Snowden";
            const string testStringWithoutUnicode = "NSA holds info over US citizens like loaded gun, but says trust me  Snowden";

            string result = Submissions.StripUnicode(testString);
            Assert.IsTrue(result.Equals(testStringWithoutUnicode));
        }

    }
}
