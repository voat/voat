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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class UrlUtilityTests
    {
        [TestMethod]
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
        public void TestGetDomainFromUri()
        {
            Uri testUri = new Uri("http://www.youtube.com");

            string result = UrlUtility.GetDomainFromUri(testUri.ToString());
            Assert.AreEqual("youtube.com", result, "Unable to extract domain from given Uri.");
        }

        [TestMethod]
        public void TestGetTitleFromUri()
        {
            const string testUri = "http://www.google.com";
            string result = UrlUtility.GetTitleFromUri(testUri);

            Assert.AreEqual("Google", result, "Unable to extract title from given Uri.");
        }

        [TestMethod]
        public void TestIsUriValid()
        {
            Uri testUri = new Uri("https://youtube.com");

            bool result = UrlUtility.IsUriValid(testUri.ToString());
            Assert.AreEqual(true, result, "The input URI was invalid.");
        }

        [TestMethod]
        public void TestEscapedQuotesInTitle()
        {
            Uri testUri = new Uri("https://lwn.net/Articles/653411/");

            string result = UrlUtility.GetTitleFromUri(testUri.ToString());
            Assert.AreEqual("\"Big data\" features coming in PostgreSQL 9.5 [LWN.net]", result, "HTML in title not properly decoded");
        }

        [TestMethod]
        public void TestTagInTitle()
        {
            Uri testUri = new Uri("http://stackoverflow.com/questions/1348683/will-the-b-and-i-tags-ever-become-deprecated");

            string result = UrlUtility.GetTitleFromUri(testUri.ToString());
            Assert.AreEqual("Will the <b> and <i> tags ever become deprecated?", result, "HTML in title not properly decoded");
        }
    }
}