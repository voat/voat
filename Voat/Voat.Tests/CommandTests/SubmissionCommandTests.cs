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

using Voat.Domain.Command;
using Voat.Tests.Repository;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class SubmissionCommandTests 
    {
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        public void CreateSubmission()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand("whatever", new Domain.Models.UserSubmission() { Title = "This is a title", Url = "http://www.yahoo.com" });

            var r = cmd.Execute().Result;

            Assert.IsTrue(r.Successfull);

            Assert.IsNotNull(r.Response);

            Assert.AreNotEqual(0, r.Response.ID);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        public void DeleteSubmission()
        {
            TestHelper.SetPrincipal("anon");

            var cmd = new DeleteSubmissionCommand(1);
            var r = cmd.Execute().Result;

            Assert.IsTrue(r.Successfull);
            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        public void Edit_Submission_Title_Content()
        {
            TestHelper.SetPrincipal("anon");

            var x = new CreateSubmissionCommand("anon", new Domain.Models.UserSubmission() { Title = "x", Content = "x" });
            var s = x.Execute().Result;
            Assert.IsTrue(s.Successfull, "Create Submission failed to return true");

            var cmd = new EditSubmissionCommand(s.Response.ID, new Domain.Models.UserSubmission() { Title = "y", Content = "y" });
            var r = cmd.Execute().Result;
            Assert.IsTrue(r.Successfull, "Edit Submission failed to return true");

            using (var repo = new Voat.Data.Repository())
            {
                var submission = repo.GetSubmission(s.Response.ID);
                Assert.IsNotNull(submission, "Can't find submission from repo");
                Assert.AreEqual("y", submission.Title);
                Assert.AreEqual("y", submission.Content);
            }

            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        public void PreventUrlTitlePosts()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand("whatever", new Domain.Models.UserSubmission() { Title = "http://www.yahoo.com", Url = "http://www.yahoo.com" });

            var r = cmd.Execute().Result;

            Assert.IsFalse(r.Successfull);

            Assert.AreEqual(r.Description, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");

            //Assert.AreNotEqual(0, r.Response.ID);
        }

    }
}
