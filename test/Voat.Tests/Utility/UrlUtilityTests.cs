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
using Voat.Utilities;
using Voat.Tests.Infrastructure;
using System.Threading.Tasks;
using System.Net;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class UrlUtilityTests : BaseUnitTest
    {
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
        [TestCategory("Utility"), TestCategory("Utility.WebRequest"), TestCategory("ExternalHttp"), TestCategory("HttpResource")]
        public async Task TestEscapedQuotesInTitle()
        {
            Uri testUri = new Uri("https://lwn.net/Articles/653411/");

            string result = null;
            using (var httpResource = new HttpResource(testUri.ToString()))
            {
                await httpResource.GiddyUp();
                result = httpResource.Title;
            }

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
        [TestCategory("Utility"), TestCategory("Utility.WebRequest"), TestCategory("ExternalHttp"), TestCategory("HttpResource")]
        public async Task TestGetTitleFromUri()
        {
            const string testUri = "http://www.google.com";
            string result = null;
            using (var httpResource = new HttpResource(testUri))
            {
                await httpResource.GiddyUp();
                result = httpResource.Title;
            }

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
        [TestCategory("Utility"), TestCategory("Utility.WebRequest"), TestCategory("ExternalHttp"), TestCategory("HttpResource")]
        public async Task TestTagInTitle()
        {
            Uri testUri = new Uri("http://stackoverflow.com/questions/1348683/will-the-b-and-i-tags-ever-become-deprecated");
            string result = null;
            using (var httpResource = new HttpResource(testUri.ToString(), new HttpResourceOptions() { AllowAutoRedirect = true }))
            {
                await httpResource.GiddyUp();
                result = httpResource.Title;
                Assert.AreEqual(true, httpResource.Redirected);
                Assert.AreEqual("Will the <b> and <i> tags ever become deprecated?", result, "HTML in title not properly decoded");
            }
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.WebRequest"), TestCategory("ExternalHttp"), TestCategory("HttpResource")]
        public async Task TestRedirect()
        {
            Uri testUri = new Uri("http://stackoverflow.com/questions/1348683/will-the-b-and-i-tags-ever-become-deprecated");
            using (var httpResource = new HttpResource(testUri.ToString()))
            {
                await httpResource.GiddyUp();
                Assert.AreEqual(false, httpResource.Redirected);
                Assert.AreEqual(HttpStatusCode.MovedPermanently, httpResource.Response.StatusCode);
            }

            using (var httpResource = new HttpResource(testUri.ToString(), new HttpResourceOptions() { AllowAutoRedirect = true }))
            {
                await httpResource.GiddyUp();
                Assert.AreEqual(true, httpResource.Redirected);
                Assert.AreEqual(HttpStatusCode.OK, httpResource.Response.StatusCode);
            }
        }
    }
}
