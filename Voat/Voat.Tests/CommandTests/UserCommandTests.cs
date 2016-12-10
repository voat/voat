using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Utilities;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class UserCommandTests : BaseUnitTest
    {

        [TestMethod]
        [TestCategory("User"), TestCategory("Command"), TestCategory("Query")]
        public async Task UpdateUserPreferences()
        {
            var userName = "TestUser5";
            var bio = Guid.NewGuid().ToString();
            TestHelper.SetPrincipal(userName);

            var q = new QueryUserPreferences(userName);
            var prefs = await q.ExecuteAsync();

            Assert.IsNotNull(prefs, "Pref query returned null");
            Assert.AreNotEqual(prefs.Bio, bio, "Bio returned unexpected data");

            var cmd = new UpdateUserPreferencesCommand(new Domain.Models.UserPreference() { Bio = bio });
            var result = await cmd.Execute();

            Assert.IsNotNull(result, "UpdatePref command returned null");
            Assert.AreEqual(true, result.Success, "UpdatePref command returned non success");

            q = new QueryUserPreferences(userName);
            prefs = await q.ExecuteAsync();

            Assert.IsNotNull(prefs, "Pref requery returned null");
            Assert.AreEqual(prefs.Bio, bio, "Bio not updated");

        }

        [TestMethod]
        [TestCategory("User"), TestCategory("Command"), TestCategory("Query")]
        public async Task UserSaves()
        {
            var userName = "UnitTestUser20";
            var bio = Guid.NewGuid().ToString();
            TestHelper.SetPrincipal(userName);

            //save start test data
            using (var repo = new Voat.Data.Repository())
            {
                var r = await repo.Save(Domain.Models.ContentType.Submission, 1);
                Assert.AreEqual(Status.Success, r.Status);

                r = await repo.Save(Domain.Models.ContentType.Comment, 1);
                Assert.AreEqual(Status.Success, r.Status);

            }

            var q = new QueryUserSaves(Domain.Models.ContentType.Submission);
            var d = await q.ExecuteAsync();
            Assert.AreEqual(true, d.Contains(1));
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual(true, CacheHandler.Instance.Exists(CachingKey.UserSavedItems(Domain.Models.ContentType.Submission, userName)));

            q = new QueryUserSaves(Domain.Models.ContentType.Comment);
            d = await q.ExecuteAsync();
            Assert.AreEqual(true, d.Contains(1));
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual(true, CacheHandler.Instance.Exists(CachingKey.UserSavedItems(Domain.Models.ContentType.Comment, userName)));


            //check helper object
            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 1));
            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 1));

            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 2));
            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 2));


            var cmd = new SaveCommand(Domain.Models.ContentType.Submission, 2);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);


            cmd = new SaveCommand(Domain.Models.ContentType.Comment, 2);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);


            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 2));
            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 2));

            cmd = new SaveCommand(Domain.Models.ContentType.Submission, 1);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);


            cmd = new SaveCommand(Domain.Models.ContentType.Comment, 1);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);

            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 1));
            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 1));

        }
    }
}
