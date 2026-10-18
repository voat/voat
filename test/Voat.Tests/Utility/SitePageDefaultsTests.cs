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
using Voat.Tests.Infrastructure;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    /// <summary>
    /// The default page bodies were converted out of Razor. These assertions
    /// guard that flattening: no Razor token may survive, and the {siteName}
    /// placeholder must be present so a rename still propagates.
    /// </summary>
    [TestClass]
    public class SitePageDefaultsTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void SitePageDefaults_BothPages_ArePresent()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(SitePageDefaults.UserAgreement), "The User Agreement default must not be empty.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(SitePageDefaults.Privacy), "The Privacy Policy default must not be empty.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void SitePageDefaults_BothPages_UseTheSiteNamePlaceholder()
        {
            Assert.IsTrue(SitePageDefaults.UserAgreement.Contains(SitePageDefaults.SiteNamePlaceholder), "The User Agreement default must reference {siteName}.");
            Assert.IsTrue(SitePageDefaults.Privacy.Contains(SitePageDefaults.SiteNamePlaceholder), "The Privacy Policy default must reference {siteName}.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void SitePageDefaults_ContainNoRazorTokens()
        {
            //Every one of these would be rendered as visible literal text by the
            //markdown renderer, which is the failure mode this guards against.
            string[] forbidden = { "@siteName", "@if", "@(", "@{", "</text>", "Html.Raw", "Model.", "ViewBag." };

            foreach (var page in new[] { SitePageDefaults.UserAgreement, SitePageDefaults.Privacy })
            {
                foreach (var token in forbidden)
                {
                    Assert.IsFalse(page.Contains(token), $"Default page content must not contain the Razor token '{token}'.");
                }
            }
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void GetDefault_KnownNames_ReturnTheMatchingPage()
        {
            Assert.AreEqual(SitePageDefaults.UserAgreement, SitePageDefaults.GetDefault("useragreement"));
            Assert.AreEqual(SitePageDefaults.Privacy, SitePageDefaults.GetDefault("privacy"));
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void GetDefault_IsCaseInsensitiveAndTrims()
        {
            Assert.AreEqual(SitePageDefaults.UserAgreement, SitePageDefaults.GetDefault("UserAgreement"), "Slugs arrive from the route and should not depend on case.");
            Assert.AreEqual(SitePageDefaults.UserAgreement, SitePageDefaults.GetDefault("  useragreement  "), "Slugs may be whitespace-padded.");
            Assert.AreEqual(SitePageDefaults.Privacy, SitePageDefaults.GetDefault("PRIVACY"));
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void GetDefault_UnknownOrEmptyName_ReturnsNull()
        {
            //Null is the slug-validation signal: UpdateHelpPage rejects the save
            //when this comes back null, so anything unrecognized must be null.
            Assert.IsNull(SitePageDefaults.GetDefault("bogus"), "An unknown slug must return null.");
            Assert.IsNull(SitePageDefaults.GetDefault(""), "An empty slug must return null.");
            Assert.IsNull(SitePageDefaults.GetDefault(null), "A null slug must return null.");
            Assert.IsNull(SitePageDefaults.GetDefault("   "), "A whitespace-only slug must return null.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void SitePageDefaults_AreMarkdownNotHtml()
        {
            //These were converted from Razor HTML. Keeping them markdown means the
            //owner's edits and the seeded defaults go through the same renderer.
            foreach (var page in new[] { SitePageDefaults.UserAgreement, SitePageDefaults.Privacy })
            {
                Assert.IsFalse(page.Contains("<h1"), "Headings must be markdown '#' lines, not <h1> tags.");
                Assert.IsFalse(page.Contains("<h2"), "Headings must be markdown '##' lines, not <h2> tags.");
                Assert.IsTrue(page.TrimStart().StartsWith("#"), "Each default page should open with a markdown heading.");
            }
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SitePage")]
        public void SitePageDefaults_KeepTheDmcaContactVerbatim()
        {
            //Deliberately left pointing at the original Voat address rather than
            //being run through {siteName}. Changing it is an owner decision.
            Assert.IsTrue(SitePageDefaults.UserAgreement.Contains("Copyright Agent copyright@voat.co"), "The DMCA contact must be preserved verbatim.");
        }
    }
}
