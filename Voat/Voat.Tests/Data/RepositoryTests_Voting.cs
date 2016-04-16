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

using System;
using Voat.Domain.Command;

namespace Voat.Tests.Repository
{


  

    [TestClass]
    public class DataGatewayTests_Voting : DatabaseRequiredUnitTest
    {
        public ContentContext context = null;

        [TestInitialize]
        public void TestInitialize()
        {
            context = ContentContext.NewContext(true);
        }


        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Down()
        {
            var x = db.GetComment(context.CommentID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User500CCP", null); //This user has one comment with 101 likes

            var response = db.VoteComment(context.CommentID, -1);

            Assert.IsTrue(response.Successfull, "Vote was not successfull");
            Assert.AreEqual(-1, response.RecordedValue, "Vote was not successfull");
            Assert.AreEqual(ups, x.UpCount);
            Assert.AreEqual((downs + 1), x.DownCount);
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Down_NoCCP()
        {
            TestHelper.SetPrincipal("TestUser3", null); //Random User has no CCP

            var response = db.VoteComment(context.CommentID, -1);

            Assert.AreEqual(Status.Denied, response.Status);
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Reset_Default()
        {
            var x = db.GetComment(context.CommentID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User100CCP", null);

            var response = db.VoteComment(context.CommentID, 1);

            Assert.IsTrue(response.Successfull, response.ToString());
            Assert.IsTrue(response.RecordedValue == 1, "Vote value incorrect");
            Assert.IsTrue(x.UpCount == (ups + 1));

            //try to re-up vote, by default should revoke vote
            response = db.VoteComment(context.CommentID, 1);
            Assert.IsTrue(response.Status == Status.Success);
            Assert.IsTrue(response.RecordedValue == 0, "Vote value incorrect");

            ////try to reset vote
            //response = repository.VoteComment(context.CommentID, 0);
            //Assert.IsTrue(response.Successfull);

            Assert.IsTrue(x.UpCount == ups);
            Assert.IsTrue(x.DownCount == downs);
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Reset_NoRevoke()
        {
            var x = db.GetComment(context.CommentID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User100CCP", null);

            var response = db.VoteComment(context.CommentID, 1);

            Assert.IsTrue(response.Successfull, response.ToString());
            Assert.IsTrue(response.RecordedValue == 1, "Vote value incorrect");
            Assert.IsTrue(x.UpCount == (ups + 1));

            //try to re-up vote, by default should revoke vote
            response = db.VoteComment(context.CommentID, 1, false);
            Assert.IsTrue(response.Status == Status.Ignored);
            Assert.IsTrue(response.RecordedValue == 1, "Vote value incorrect");
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Up()
        {
            var x = db.GetComment(context.CommentID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User50CCP", null);

            var response = db.VoteComment(context.CommentID, 1);

            Assert.IsTrue(response.Successfull, response.SystemDescription);
            Assert.IsTrue(x.UpCount == (ups + 1));
            Assert.IsTrue(x.DownCount == downs);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCategory("Repository"), TestCategory("Repository.Vote")]
        public void EnsureInvalidVoteValueThrowsException_Com()
        {
            db.VoteComment(121, -2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCategory("Repository"), TestCategory("Repository.Vote")]
        public void EnsureInvalidVoteValueThrowsException_Sub()
        {
            db.VoteSubmission(1, 21);
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Down()
        {
            var x = db.GetSubmission(context.SubmissionID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User500CCP", null); //This user has one comment with 101 likes

            var response = db.VoteSubmission(context.SubmissionID, -1);

            Assert.IsTrue(response.Successfull, "Vote was not successfull");
            Assert.IsTrue(response.RecordedValue == -1, "Vote was not successfull");
            Assert.IsTrue(x.UpCount == ups);
            Assert.IsTrue(x.DownCount == (downs + 1));
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Down_NoCCP()
        {
            TestHelper.SetPrincipal("TestUser3", null); //Random User has no CCP

            var response = db.VoteSubmission(context.SubmissionID, -1);

            Assert.AreEqual(Status.Denied, response.Status);
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Reset_Default()
        {
            var x = db.GetSubmission(context.SubmissionID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User100CCP", null);

            var response = db.VoteSubmission(context.SubmissionID, 1);

            Assert.IsTrue(response.Successfull, response.ToString());
            Assert.IsTrue(response.RecordedValue == 1, "Vote value incorrect");
            Assert.IsTrue(x.UpCount == (ups + 1));

            //try to re-up vote
            response = db.VoteSubmission(context.SubmissionID, 1);
            Assert.IsTrue(response.Status == Status.Success);
            Assert.IsTrue(response.RecordedValue == 0);
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Reset_NoRevoke()
        {
            var x = db.GetSubmission(context.SubmissionID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User100CCP", null);

            var response = db.VoteSubmission(context.SubmissionID, 1);

            Assert.IsTrue(response.Successfull, response.ToString());
            Assert.IsTrue(response.RecordedValue == 1, "Vote value incorrect");
            Assert.IsTrue(x.UpCount == (ups + 1));

            //try to re-up vote
            response = db.VoteSubmission(context.SubmissionID, 1, false);
            Assert.IsTrue(response.Status == Status.Ignored); //setting tells voting to ignore if submitted vote is current vote
            Assert.IsTrue(response.RecordedValue == 1); //should still be an upvote
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Up()
        {
            var x = db.GetSubmission(context.SubmissionID);
            var ups = x.UpCount;
            var downs = x.DownCount;

            TestHelper.SetPrincipal("User50CCP", null);

            var response = db.VoteSubmission(context.SubmissionID, 1);

            Assert.IsTrue(response.Successfull, response.SystemDescription);
            Assert.IsTrue(response.RecordedValue == 1, response.SystemDescription);
            Assert.IsTrue(x.UpCount == (ups + 1));
            Assert.IsTrue(x.DownCount == downs);
        }

    }
}
