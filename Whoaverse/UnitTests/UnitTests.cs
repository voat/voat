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
            Assert.AreEqual(143, result, 0.1, "Submission double age was not calculated.");
        }

        [TestMethod]
        public void TestGetDomainFromUri()
        {
            Uri testUri = new Uri("http://www.youtube.com");

            string result = Badges.GetDomainFromUri(testUri.ToString());
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
