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
using Voat.Configuration;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Utils
{
    /// <summary>
    /// IsSiteOwner gates editing of the User Agreement and Privacy Policy. It
    /// must fail closed: anything not explicitly listed is denied, including
    /// null, empty and unknown accounts.
    ///
    /// These build a local VoatSettings rather than touching VoatSettings.Instance,
    /// which is shared across the test run.
    /// </summary>
    [TestClass]
    public class SiteOwnerTests : BaseUnitTest
    {
        private static VoatSettings SettingsWith(params string[] owners)
        {
            return new VoatSettings { SiteOwners = owners };
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_ListedAccount_IsAllowed()
        {
            var settings = SettingsWith("admin");
            Assert.IsTrue(settings.IsSiteOwner("admin"), "A listed account should be an owner.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_IsCaseInsensitive()
        {
            var settings = SettingsWith("Admin");

            Assert.IsTrue(settings.IsSiteOwner("admin"), "Matching should not depend on case (stored form).");
            Assert.IsTrue(settings.IsSiteOwner("ADMIN"), "Matching should not depend on case (query form).");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_UnlistedAccount_IsDenied()
        {
            var settings = SettingsWith("admin");

            Assert.IsFalse(settings.IsSiteOwner("mallory"), "An account not on the list must be denied.");
            Assert.IsFalse(settings.IsSiteOwner("admin2"), "A near-miss name must not match.");
            Assert.IsFalse(settings.IsSiteOwner("adm"), "A prefix of a listed name must not match.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_EmptyOrNullName_IsDenied()
        {
            var settings = SettingsWith("admin");

            Assert.IsFalse(settings.IsSiteOwner(null), "A null user name must be denied.");
            Assert.IsFalse(settings.IsSiteOwner(""), "An empty user name must be denied.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_EmptyList_FailsClosed()
        {
            Assert.IsFalse(SettingsWith().IsSiteOwner("admin"), "An empty list must leave editing inert.");
            Assert.IsFalse(SettingsWith().IsSiteOwner(null), "An empty list must leave editing inert.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_NullList_FailsClosed()
        {
            var settings = new VoatSettings { SiteOwners = null };

            Assert.IsFalse(settings.IsSiteOwner("admin"), "A null list must leave editing inert.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_BlankEntries_AreSkippedNotMatched()
        {
            var settings = SettingsWith("", "   ", null, "admin");

            Assert.IsTrue(settings.IsSiteOwner("admin"), "Blank entries should be skipped, not treated as a match.");
            Assert.IsFalse(settings.IsSiteOwner(""), "A blank entry must never match a blank user name.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_CommaSeparatedEntry_SplitsIntoSeveralOwners()
        {
            //This is the docker-compose shape: one environment variable naming
            //several accounts, e.g. VOAT_SITE_OWNERS=admin,otheruser.
            var settings = SettingsWith("admin,otheruser");

            Assert.IsTrue(settings.IsSiteOwner("admin"), "The first name in a comma-separated entry should match.");
            Assert.IsTrue(settings.IsSiteOwner("otheruser"), "The second name in a comma-separated entry should match.");
            Assert.IsFalse(settings.IsSiteOwner("admin,otheruser"), "The raw entry must not match as one name.");
            Assert.IsFalse(settings.IsSiteOwner("other"), "A partial segment must not match.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_CommaSeparatedEntry_TrimsWhitespaceAroundNames()
        {
            var settings = SettingsWith("admin, otheruser ,third");

            Assert.IsTrue(settings.IsSiteOwner("otheruser"), "Names after the first are often space-padded.");
            Assert.IsTrue(settings.IsSiteOwner("third"), "Names before the closing comma are often space-padded.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.SiteOwner")]
        public void SiteOwner_ManyEntries_AllMatch()
        {
            var settings = SettingsWith("admin", "otheruser");

            Assert.IsTrue(settings.IsSiteOwner("admin"));
            Assert.IsTrue(settings.IsSiteOwner("otheruser"));
            Assert.IsFalse(settings.IsSiteOwner("mallory"));
        }
    }
}
