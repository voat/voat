using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Domain.Query;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class UserCommandTests
    {

        [TestMethod]
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
    }
}
