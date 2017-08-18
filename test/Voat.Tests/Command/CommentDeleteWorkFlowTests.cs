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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Common;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests
{

    [TestClass]
    public partial class CommentDeleteWorkFlowTests : BaseUnitTest
    {
        private string AUTHOR_NAME = "TestUser13";
        private string MOD_NAME = USERNAMES.Unit;
        private string CONTENT = Guid.NewGuid().ToString();
        private int SUBMISSION_ID = 0;
        private static Tuple<int, int> CommentIDs;

        public override void ClassInitialize()
        {
            
            var user = TestHelper.SetPrincipal(AUTHOR_NAME);

            //create submission
            var s = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Content = CONTENT, Title = "My Title Goes Here", Subverse = SUBVERSES.Unit }).SetUserContext(user);
            var r1 = s.Execute().Result;
            VoatAssert.IsValid(r1);
            SUBMISSION_ID = r1.Response.ID;

            //create user comment 1
            var c = new CreateCommentCommand(r1.Response.ID, null, CONTENT + "1").SetUserContext(user);
            var rc = c.Execute().Result;
            VoatAssert.IsValid(rc);

            //create user comment 2
            var c2 = new CreateCommentCommand(r1.Response.ID, null, CONTENT + "2").SetUserContext(user);
            var rc2 = c2.Execute().Result;
            VoatAssert.IsValid(rc2);

            CommentIDs = Tuple.Create(rc.Response.ID, rc2.Response.ID);

            //Add comments to parents so segment gets returned.
            user = TestHelper.SetPrincipal(MOD_NAME);
            var d1 = new CreateCommentCommand(r1.Response.ID, CommentIDs.Item1, "Gunna so delete this").SetUserContext(user);
            var rd2 = d1.Execute().Result;
            VoatAssert.IsValid(rd2);

            d1 = new CreateCommentCommand(r1.Response.ID, CommentIDs.Item2, "Gunna so delete this too!").SetUserContext(user);
            rd2 = d1.Execute().Result;
            VoatAssert.IsValid(rd2);


        }
        [TestMethod]
        [TestCategory("Process"), TestCategory("Comment"), TestCategory("Command.Comment"), TestCategory("Query.Comment")]
        public void TestCommentDelete()
        {
            var authorCommentID = CommentIDs.Item1;
            var modCommentID = CommentIDs.Item2;

            //read content, ensure data is correct

            VerifyComment(authorCommentID, CONTENT, false);
            VerifyComment(modCommentID, CONTENT, false);

            //delete comment (mod)
            var user = TestHelper.SetPrincipal(AUTHOR_NAME);
            var d = new DeleteCommentCommand(authorCommentID).SetUserContext(user);
            var r = d.Execute().Result;
            Assert.IsTrue(r.Success, r.Message);

            user = TestHelper.SetPrincipal(MOD_NAME);
            d = new DeleteCommentCommand(modCommentID, "My Reason Here").SetUserContext(user); ;
            r = d.Execute().Result;
            Assert.IsTrue(r.Success, r.Message);

            //read comment, ensure data exists
            VerifyComment(authorCommentID, "Deleted", true);
            VerifyComment(modCommentID, "Deleted", true);

        }
        private void VerifyComment(int commentID, string expectedContent, bool isDeleted)
        {
            var queryCommentDirect = new QueryComment(commentID, Caching.CachePolicy.None);
            var comment = queryCommentDirect.Execute();
            Assert.IsNotNull(comment);
            Assert.IsTrue(comment.Content.Contains(expectedContent));
            Assert.AreEqual(isDeleted, comment.IsDeleted);

            //purge cache to ensure fresh pull
            Caching.CacheHandler.Instance.Remove(Caching.CachingKey.CommentTree(comment.SubmissionID.Value));

            var queryCommentTree = new QueryCommentSegment(comment.SubmissionID.Value);
            var tree = queryCommentTree.Execute();

            var treeComment = tree.Comments.FirstOrDefault(x => x.ID == commentID);
            Assert.IsNotNull(treeComment);
            Assert.IsTrue(treeComment.Content.Contains(expectedContent));
            Assert.AreEqual(isDeleted, treeComment.IsDeleted);


        }

    }
}
