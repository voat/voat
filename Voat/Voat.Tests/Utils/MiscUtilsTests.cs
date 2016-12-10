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

using OpenGraph_Net;
using System;
using System.Collections.Generic;
using System.Linq;
using Voat.Common;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class MiscUtilsTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.Calculation")]
        public void TestCalcRank()
        {
            double result = Ranking.CalculateNewRank(0.5, 150, 20);
            Assert.AreEqual(0.0012465, result, 0.01, "Rank was not calculated.");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void TestFormatMarkdown()
        {
            string testString = "**Bold**";

            string result = Formatting.FormatMessage(testString);
            Assert.AreEqual("<p><strong>Bold</strong></p>", result.Trim(), "Markdown formatting failed");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.WebRequest")]
        public void TestGetDomainFromUri()
        {
            Uri testUri = new Uri("http://www.youtube.com");

            string result = UrlUtility.GetDomainFromUri(testUri.ToString());
            Assert.AreEqual("youtube.com", result, "Unable to extract domain from given Uri.");
        }

        //[Ignore] //This fails often, ignoring.
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.WebRequest")]
        public void TestGetOpenGraphImageFromUri()
        {
            //HACK: This test is most likely Geo Sensitive, thus it fails on a U.S. network.
            //This test needs to be performed on static server based resource instead.
            Uri testUri = new Uri("http://www.bbc.com/news/technology-32194196");
            var graph = OpenGraph.ParseUrl(testUri);

            List<string> acceptable = new List<string>() {
                "http://ichef.bbci.co.uk/news/1024/media/images/80755000/jpg/_80755021_163765270.jpg", //'merica test
                "http://ichef-1.bbci.co.uk/news/1024/media/images/80755000/jpg/_80755021_163765270.jpg", //'merica test part 2
                "http://news.bbcimg.co.uk/media/images/80755000/jpg/_80755021_163765270.jpg" //Yuro test
            };
            var expected = graph.Image.ToString();

            var passed = acceptable.Any(x => x.Equals(expected, StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(passed, "OpenGraph was unable to find an acceptable image path");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.WebRequest")]
        public void TestGetTitleFromUri()
        {
            const string testUri = "http://www.google.com";
            string result = UrlUtility.GetTitleFromUri(testUri);

            Assert.AreEqual("Google", result, "Unable to extract title from given Uri.");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.Calculation")]
        public void TestScoreBehaviorObject()
        {
            Score vb;
            vb = new Score() { UpCount = 10, DownCount = 10 };
            Assert.IsTrue(vb.Total == 20, "t0.1");
            Assert.IsTrue(vb.UpCount == 10, "t0.2");
            Assert.IsTrue(vb.DownCount == 10, "t0.3");
            Assert.IsTrue(vb.Bias == 1f, "t0.4");
            Assert.IsTrue(vb.UpRatio == 0.5f, "t0.5");
            Assert.IsTrue(vb.DownRatio == 0.5f, "t0.6");
            Assert.IsTrue(vb.UpRatio + vb.DownRatio == 1.0f, "t0.7");

            vb = new Score() { UpCount = 10, DownCount = 5 };
            Assert.IsTrue(vb.Total == 15, "t1.1");
            Assert.IsTrue(vb.UpCount == 10, "t1.2");
            Assert.IsTrue(vb.DownCount == 5, "t1.3");
            Assert.IsTrue(vb.Bias == 2, "t1.4");
            Assert.IsTrue(vb.UpRatio == 0.67, "t1.5");
            Assert.IsTrue(vb.DownRatio == 0.33, "t1.6");

            //ensure negatives aren't stored
            vb = new Score() { UpCount = -10, DownCount = -10 };
            Assert.IsTrue(vb.Total == 0, "t2.1");
            Assert.IsTrue(vb.UpCount == 0, "t2.2");
            Assert.IsTrue(vb.DownCount == 0, "t2.3");
            Assert.IsTrue(vb.Bias == 1, "t2.4");

            vb = (new Score() { UpCount = 10, DownCount = 5 })
                .Combine(new Score() { UpCount = 10, DownCount = 5 })
                .Combine(new Score() { UpCount = 10, DownCount = 5 });
            Assert.IsTrue(vb.Total == 45, "t3.1");
            Assert.IsTrue(vb.UpCount == 30, "t3.2");
            Assert.IsTrue(vb.DownCount == 15, "t3.3");
            Assert.IsTrue(vb.Bias == 2, "t3.4");
        }
        //"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\n\n\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇ"

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeDetection1()
        {
            const string testString = "🆆🅰🆂 🅶🅴🆃🆃🅸🅽🅶 🅲🅰🆄🅶🅷🆃 🅿🅰🆁🆃 🅾🅵 🆈🅾🆄🆁 🅿🅻🅰🅽🅴";
            const string testStringWithoutUnicode = "was getting caught part of your plane";

            bool result = Formatting.ContainsUnicode(testString);
            Assert.IsTrue(result, "Unicode was not detected.");

            bool resultWithoutUnicode = Formatting.ContainsUnicode(testStringWithoutUnicode);
            Assert.IsFalse(resultWithoutUnicode, "Unicode was not detected.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeDetection2()
        {
            const string testString = "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\n\n\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇ";

            bool result = Formatting.ContainsUnicode(testString);
            Assert.IsTrue(result, "Unicode was not detected.");

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeDetection3()
        {
            const string testString = "ÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
            bool result = Formatting.ContainsUnicode(testString);
            Assert.IsFalse(result, "Unicode was detected.");
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestTitleStriping1()
        {
            const string testString = "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\n\n\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f";
            var result = Formatting.StripUnicode(testString);
            Assert.AreEqual("", result);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestTitleStriping2()
        {
            const string testString = "ÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
            var result = Formatting.StripUnicode(testString);
            Assert.AreNotEqual("", result);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeStripping1()
        {
            const string testString = "NSA holds info over US citizens like loaded gun, but says ‘trust me’ – Snowden";
            const string testStringWithoutUnicode = "NSA holds info over US citizens like loaded gun, but says trust me Snowden";

            string result = Formatting.StripUnicode(testString);
            Assert.IsTrue(result.Equals(testStringWithoutUnicode));
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeStripping2()
        {
            string testString = "|       |";
            string result = Formatting.StripWhiteSpace(testString);
            Assert.AreEqual("| |", result, "Multiple whitespace strip");

            testString = "| |";
            result = Formatting.StripWhiteSpace(testString);
            Assert.AreEqual("| |", result, "Single whitespace strip");

            testString = " | |";
            result = Formatting.StripWhiteSpace(testString);
            Assert.AreEqual("| |", result, "Leading whitespace strip");

            testString = "|     |  ";
            result = Formatting.StripWhiteSpace(testString);
            Assert.AreEqual("| |", result, "Trailing whitespace strip");

            testString = null;
            result = Formatting.StripWhiteSpace(testString);
            Assert.IsNull(result, "Null should return null");
        }
    }
}
