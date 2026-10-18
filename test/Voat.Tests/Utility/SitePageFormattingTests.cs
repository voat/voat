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
using System.Net;
using Voat.Configuration;
using Voat.Tests.Infrastructure;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    /// <summary>
    /// FormatSitePageMessage is what _HelpPage.cshtml calls to turn the stored
    /// markdown into HTML. These exercise exactly that call against the real
    /// default page bodies, so a throw here is the same throw the page would
    /// make.
    /// </summary>
    [TestClass]
    public class SitePageFormattingTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_RendersBothDefaultPages()
        {
            foreach (var name in new[] { "useragreement", "privacy" })
            {
                var markdown = SitePageDefaults.GetDefault(name);
                Assert.IsNotNull(markdown, $"GetDefault(\"{name}\") must return content.");

                var html = Formatting.FormatSitePageMessage(markdown);

                Assert.IsNotNull(html, $"FormatSitePageMessage must return HTML for {name}.");
                Assert.IsTrue(html.Length > 0, $"FormatSitePageMessage must not return empty HTML for {name}.");
            }
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_SubstitutesTheSiteNamePlaceholder()
        {
            var html = Formatting.FormatSitePageMessage("Hello {siteName}.");

            Assert.IsFalse(html.Contains("{siteName}"), "The placeholder must not survive into the HTML.");
            StringAssert.Contains(html, WebUtility.HtmlEncode(VoatSettings.Instance.SiteName), "The site name must be substituted in.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_SubstitutesIntoTheDefaultPages()
        {
            foreach (var name in new[] { "useragreement", "privacy" })
            {
                var html = Formatting.FormatSitePageMessage(SitePageDefaults.GetDefault(name));

                Assert.IsFalse(html.Contains("{siteName}"), $"No placeholder may remain in the rendered {name}.");
                Assert.IsFalse(html.Contains("@siteName"), $"No Razor token may remain in the rendered {name}.");
            }
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_SubstitutesTheSiteNameEncoded()
        {
            //The placeholder is replaced with the HtmlEncoded brand AFTER markdown
            //runs, so a name containing markdown punctuation or an ampersand cannot
            //alter the output. Output must be exactly one paragraph.
            string html = Formatting.FormatSitePageMessage("{siteName}");

            Assert.AreEqual(
                "<p>" + WebUtility.HtmlEncode(VoatSettings.Instance.SiteName) + "</p>",
                html.Trim(),
                "The brand must land inside the paragraph, HTML-encoded.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_EscapesRawHtml()
        {
            //SafeMode: the owner's markdown is legal text, and a saved <script>
            //must render as visible escaped text, not execute.
            string html = Formatting.FormatSitePageMessage("hello <script>alert(1)</script> world");

            Assert.IsFalse(html.Contains("<script>"), "Raw script tags must not reach the output unescaped.");
            StringAssert.Contains(html, "&lt;script&gt;", "The tag should be visible as escaped text.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_EmptyOrNullInput_DoesNotThrow()
        {
            //A page row whose Content is null falls back before formatting, but
            //this must not be the thing that takes the page down.
            Assert.IsNotNull(Formatting.FormatSitePageMessage(null));
            Assert.IsNotNull(Formatting.FormatSitePageMessage(""));
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_DoesNotAutoLinkSocialTokens()
        {
            //processContent is off: the autolinker would rewrite these into links.
            string html = Formatting.FormatSitePageMessage("contact @admin in v/somesub or /r/reddit");

            Assert.IsFalse(html.Contains("<a "), "Social tokens must not be auto-linked in legal copy.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_KeepsOrdinaryMarkdownLinks()
        {
            string html = Formatting.FormatSitePageMessage("[Google Privacy Policy](http://www.google.com/policies/privacy/partners/)");

            StringAssert.Contains(html, "<a ", "A written markdown link must still render as a link.");
            StringAssert.Contains(html, "http://www.google.com/policies/privacy/partners/", "The href must survive.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage"), TestCategory("Formatting")]
        public void FormatSitePageMessage_StillBlocksJavascriptLinks()
        {
            //The href must be neutralized. Note the formatter also echoes the
            //blocked url into a data-ScriptStrip attribute for verification, so
            //assert on the href rather than on the whole document.
            string html = Formatting.FormatSitePageMessage("[x](javascript:alert(1))");

            Assert.IsFalse(html.Contains("href=\"javascript:"), "An injected javascript: href must not survive.");
            Assert.IsFalse(html.Contains("href='javascript:"), "An injected javascript: href must not survive.");
            StringAssert.Contains(html, "href=\"#\"", "The blocked link should be neutralized to '#'.");
        }
    }
}
