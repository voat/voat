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
    public class CommentCommandTests 
    {
        #region Anon Comment Tests

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment_Anon()
        {
            TestHelper.SetPrincipal("TestUser1");

            var cmd = new CreateCommentCommand(2, null, "This is my data");
            var c = cmd.Execute().Result;

            Assert.IsTrue(c.Success);
            Assert.IsNotNull(c.Response);
            Assert.AreNotEqual(0, c.Response.ID);
            Assert.AreEqual(true, c.Response.IsAnonymized);
            Assert.AreNotEqual(cmd.Content, c.Response.FormattedContent);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(c.Response.ID);
                Assert.IsNotNull(comment, "Couldn't find comment in db", c.Response.ID);

                Assert.AreEqual(c.Response.ID, comment.ID);
                Assert.AreEqual(comment.ID.ToString(), c.Response.UserName);
                Assert.AreEqual(c.Response.Content, comment.Content);
                Assert.IsTrue(comment.IsAnonymized);
                Assert.AreEqual(c.Response.IsAnonymized, comment.IsAnonymized);
            }
        }

        #endregion Anon Comment Tests

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateCommentCommand(1, null, "This is my data");
            var c = cmd.Execute().Result;

            Assert.IsTrue(c.Success);
            Assert.IsNotNull(c.Response);
            Assert.AreNotEqual(0, c.Response.ID);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(c.Response.ID);
                Assert.IsNotNull(comment, "Couldn't find comment in db", c.Response.ID);
                Assert.AreEqual(c.Response.ID, comment.ID);
                Assert.AreEqual(c.Response.UserName, comment.UserName);
                Assert.AreEqual(c.Response.Content, comment.Content);
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment_WithBannedSubUser()
        {
            TestHelper.SetPrincipal("BannedFromVUnit");

            var cmd = new CreateCommentCommand(1, null, "This is my data with banned user");
            var c = cmd.Execute().Result;

            Assert.IsFalse(c.Success, "User should be banned from commenting");
            Assert.AreEqual(Status.Denied, c.Status);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment_WithGloballyBannedUser()
        {
            TestHelper.SetPrincipal("BannedGlobally");

            var cmd = new CreateCommentCommand(1, null, "This is my data");
            var c = cmd.Execute().Result;

            Assert.IsFalse(c.Success, "User should be banned from commenting");
            Assert.AreEqual(Status.Denied, c.Status);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void DeleteComment()
        {
            TestHelper.SetPrincipal("TestUser1");
            var cmdcreate = new CreateCommentCommand(1, null, "This is my data too you know");
            var c = cmdcreate.Execute().Result;

            Assert.IsNotNull(c, "response null");
            Assert.IsTrue(c.Success, c.Message);
            Assert.IsNotNull(c.Response, "Response payload null");

            int id = c.Response.ID;

            var cmd = new DeleteCommentCommand(id);
            var r = cmd.Execute().Result;
            Assert.IsTrue(r.Success);

            //verify
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(id);
                Assert.AreEqual(true, comment.IsDeleted);
                Assert.AreNotEqual(c.Response.Content, comment.Content);
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void EditComment()
        {
            TestHelper.SetPrincipal("unit");
            var cmd = new EditCommentCommand(1, "This is data [howdy](http://www.howdy.com)");
            var r = cmd.Execute().Result;
            Assert.IsTrue(r.Success);

            //verify
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(1);
                Assert.IsNotNull(comment.LastEditDate);
                Assert.AreEqual(cmd.Content, comment.Content);
            }
        }
    }
}
