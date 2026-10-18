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
using System.Threading.Tasks;
using Voat.Data;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Repository
{
    /// <summary>
    /// SitePage is the storage behind the owner-editable User Agreement and
    /// Privacy Policy. A page that has never been saved must read as null so the
    /// caller can fall back to SitePageDefaults; a saved page must round-trip and
    /// a later save must update the same row rather than add another.
    /// </summary>
    [TestClass]
    public class SitePageRepositoryTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.SitePage")]
        public async Task SitePage_SaveThenGet_RoundTrips()
        {
            var user = TestHelper.SetPrincipal("TestUser01");
            string name = "useragreement";
            string content = "# Test agreement\n\nBody text. **bold** {siteName}.";

            using (var repo = new Voat.Data.Repository(user))
            {
                await repo.SaveSitePage(name, content, "TestUser01");
            }

            using (var repo = new Voat.Data.Repository(user))
            {
                var page = await repo.GetSitePage(name);

                Assert.IsNotNull(page, "A page that was just saved must come back.");
                Assert.AreEqual(name, page.Name, "The stored slug must round-trip.");
                Assert.AreEqual(content, page.Content, "The stored markdown must round-trip unchanged.");
                Assert.AreEqual("TestUser01", page.UpdatedBy, "The saving account must be stamped.");
                Assert.IsTrue(page.LastUpdateDate.HasValue, "A saved page must carry a timestamp.");
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.SitePage")]
        public async Task SitePage_GetUnsavedName_ReturnsNull()
        {
            var user = TestHelper.SetPrincipal("TestUser01");

            //A name that has never been written. The GUID keeps this from
            //colliding with a row left behind by an earlier run.
            string name = "sitpagemissing" + Guid.NewGuid().ToString("N").Substring(0, 12);

            using (var repo = new Voat.Data.Repository(user))
            {
                var page = await repo.GetSitePage(name);

                Assert.IsNull(page, "An unknown name must return null — that is the caller's fallback-to-defaults signal.");
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.SitePage")]
        public async Task SitePage_SecondSave_UpdatesInPlace()
        {
            var user = TestHelper.SetPrincipal("TestUser01");
            string name = "privacy";

            using (var repo = new Voat.Data.Repository(user))
            {
                await repo.SaveSitePage(name, "first draft", "TestUser01");
            }

            string firstContent;
            using (var repo = new Voat.Data.Repository(user))
            {
                firstContent = (await repo.GetSitePage(name)).Content;
            }

            using (var repo = new Voat.Data.Repository(user))
            {
                await repo.SaveSitePage(name, "second draft", "TestUser01");
            }

            using (var repo = new Voat.Data.Repository(user))
            {
                var page = await repo.GetSitePage(name);

                Assert.IsNotNull(page);
                Assert.AreEqual("second draft", page.Content, "A later save must win.");
                Assert.AreNotEqual(firstContent, page.Content, "The earlier draft must not still be the stored content.");
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.SitePage")]
        public async Task SitePage_Save_StampsTheRequestingUserAndTime()
        {
            var user = TestHelper.SetPrincipal("TestUser01");
            string name = "useragreement";

            using (var repo = new Voat.Data.Repository(user))
            {
                await repo.SaveSitePage(name, "stamp check one", "TestUser01");
            }

            DateTime firstStamp;
            using (var repo = new Voat.Data.Repository(user))
            {
                firstStamp = (await repo.GetSitePage(name)).LastUpdateDate.Value;
            }

            using (var repo = new Voat.Data.Repository(user))
            {
                await repo.SaveSitePage(name, "stamp check two", "TestUser02");
            }

            using (var repo = new Voat.Data.Repository(user))
            {
                var page = await repo.GetSitePage(name);

                Assert.AreEqual("TestUser02", page.UpdatedBy, "A later save must overwrite the author with the new editor.");
                Assert.IsTrue(page.LastUpdateDate.Value >= firstStamp, "A later save must move the timestamp forward.");
            }
        }
    }
}
