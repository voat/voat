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
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class UrlUtilityTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility")]
        public void GetYoutubeIdFromUrl()
        {
            var expected = "Gjk5udJY_gM";

            var youtubeUrl = "https://www.youtube.com/watch?v=Gjk5udJY_gM";
            var output = UrlUtility.GetVideoIdFromUrl(youtubeUrl);
            Assert.AreEqual(expected, output);

            var miniUrl = "https://youtu.be/Gjk5udJY_gM?t=11s";
            output = UrlUtility.GetVideoIdFromUrl(miniUrl);
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        [TestCategory("Utility")]
        public void TrapInjectableJavascript()
        {
            var url = "javascript: alert(1);void(​0));";
            Assert.IsTrue(UrlUtility.InjectableJavascriptDetected(url), url);

            url = " JAVASCRIPT : alert(1);void(​0));";
            Assert.IsTrue(UrlUtility.InjectableJavascriptDetected(url), url);

            url = "&#106;avascript:alert(1);void(​0);";
            Assert.IsTrue(UrlUtility.InjectableJavascriptDetected(url), url);

            url = "http://javascript.com/someurl";
            Assert.IsFalse(UrlUtility.InjectableJavascriptDetected(url), url);

            url = "https://preview.voat.co/v/test/comments/1088839/5419812";
            Assert.IsFalse(UrlUtility.InjectableJavascriptDetected(url), url);

        }



        [TestMethod]
        [TestCategory("Utility")]
        public void TestEscapedQuotesInTitle()
        {
            Uri testUri = new Uri("https://lwn.net/Articles/653411/");

            string result = UrlUtility.GetTitleFromUri(testUri.ToString());
            Assert.AreEqual("\"Big data\" features coming in PostgreSQL 9.5 [LWN.net]", result, "HTML in title not properly decoded");
        }

        [TestMethod]
        [TestCategory("Utility")]
        public void TestGetDomainFromUri()
        {
            Uri testUri = new Uri("http://www.youtube.com");

            string result = UrlUtility.GetDomainFromUri(testUri.ToString());
            Assert.AreEqual("youtube.com", result, "Unable to extract domain from given Uri.");
        }

        [TestMethod]
        [TestCategory("Utility")]
        public void TestGetTitleFromUri()
        {
            const string testUri = "http://www.google.com";
            string result = UrlUtility.GetTitleFromUri(testUri);

            Assert.AreEqual("Google", result, "Unable to extract title from given Uri.");
        }

        [TestCategory("Utility")]
        [TestMethod]
        public void TestIsUriValid()
        {
            Uri testUri = new Uri("https://youtube.com");

            bool result = UrlUtility.IsUriValid(testUri.ToString());
            Assert.AreEqual(true, result, "The input URI was invalid.");
        }
        [TestCategory("Utility")]
        [TestMethod]
        public void TestIsUriValid2()
        {
            Uri testUri = new Uri("http://😍.😍");

            bool result = UrlUtility.IsUriValid(testUri.ToString(), false);
            Assert.AreEqual(true, result, "The input URI was invalid 1");

            result = UrlUtility.IsUriValid(testUri.ToString(), true);
            Assert.AreEqual(false, result, "The input URI was invalid 2");

            result = UrlUtility.IsUriValid(testUri.ToString());
            Assert.AreEqual(false, result, "The input URI was invalid 3");
        }
        [TestCategory("Utility")]
        [TestMethod]
        public void TestIsUriValid3()
        {
            Uri testUri = new Uri("http://​.​");

            bool result = UrlUtility.IsUriValid(testUri.ToString(), false);
            Assert.AreEqual(true, result, "The input URI was invalid 1");

            result = UrlUtility.IsUriValid(testUri.ToString(), true);
            Assert.AreEqual(false, result, "The input URI was invalid 2");

            result = UrlUtility.IsUriValid(testUri.ToString());
            Assert.AreEqual(false, result, "The input URI was invalid 3");
        }
        [TestMethod]
        [TestCategory("Utility")]
        public void TestTagInTitle()
        {
            Uri testUri = new Uri("http://stackoverflow.com/questions/1348683/will-the-b-and-i-tags-ever-become-deprecated");

            string result = UrlUtility.GetTitleFromUri(testUri.ToString());
            Assert.AreEqual("Will the <b> and <i> tags ever become deprecated?", result, "HTML in title not properly decoded");
        }
    }
}
