/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

namespace Utility.Tests
{
    using System;
    using Voat.Utils;
    using Xunit;

    public class MiscTests
    {

        [Fact]
        public void TestGetDomainFromUri()
        {
            Uri testUri = new Uri("http://www.youtube.com");

            string result = UrlUtility.GetDomainFromUri(testUri.ToString());
            Assert.Equal("youtube.com", result);
        }

        [Fact(Skip = "It's essentially an integration test, static dependency calls OpenGraph.  TODO: refactor")]
        public void TestGetTitleFromUri()
        {
            const string testUri = "http://www.google.com";
            string result = UrlUtility.GetTitleFromUri(testUri);

            Assert.Equal("Google", result);
        }
/*
        Testing one external library call seems a bit superfluous...
  
        [Fact]
        public void TestGetOpenGraphImageFromUri()
        {
            Uri testUri = new Uri("http://www.bbc.com/news/technology-32194196");
            var graph = OpenGraph.ParseUrl(testUri);
            Assert.AreEqual("http://ichef.bbci.co.uk/news/1024/media/images/80755000/jpg/_80755021_163765270.jpg", graph.Image.ToString(), "Unable to extract domain from given Uri.");
        }
*/

        [Fact(Skip = "HttpContext is not available from unit test project and there's a static dependency on it. TODO: refactor")]
        public void TestFormatMarkdown()
        {
            string testString = "**Bold**";

            string result = Formatting.FormatMessage(testString);
            Assert.Equal("<p><strong>Bold</strong></p>", result.Trim());
        }

        [Fact]
        public void TestCalcRank()
        {
            double result = Ranking.CalculateNewRank(0.5, 150, 20);
            Assert.Equal(0.0012465, result);
        }

        [Fact]
        public void TestUnicodeDetection()
        {
            const string testString = "🆆🅰🆂 🅶🅴🆃🆃🅸🅽🅶 🅲🅰🆄🅶🅷🆃 🅿🅰🆁🆃 🅾🅵 🆈🅾🆄🆁 🅿🅻🅰🅽🅴";
            const string testStringWithoutUnicode = "was getting caught part of your plane";

            bool result = Submissions.ContainsUnicode(testString);
            Assert.True(result, "Unicode was not detected.");

            bool resultWithoutUnicode = Submissions.ContainsUnicode(testStringWithoutUnicode);
            Assert.False(resultWithoutUnicode, "Unicode was not detected.");
        }

        [Fact]
        public void TestUnicodeStripping()
        {
            const string testString = "NSA holds info over US citizens like loaded gun, but says ‘trust me’ – Snowden";
            const string testStringWithoutUnicode = "NSA holds info over US citizens like loaded gun, but says trust me  Snowden";

            string result = Submissions.StripUnicode(testString);
            Assert.True(result.Equals(testStringWithoutUnicode));
        }

    }
}
