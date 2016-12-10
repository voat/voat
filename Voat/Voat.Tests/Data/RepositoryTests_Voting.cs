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
using System.Linq;
using Voat.Domain.Command;
using Voat.Utilities;

namespace Voat.Tests.Repository
{
    [TestClass]
    public class RepositoryTests_Voting : BaseUnitTest
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
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;
                string userName = "User500CCP";
                TestHelper.SetPrincipal(userName, null); //This user has one comment with 101 likes

                var response = db.VoteComment(context.CommentID, -1, IpHash.CreateHash("127.0.0.1"));
                Assert.IsTrue(response.Success, "Vote was not successfull");
                Assert.AreEqual(-1, response.RecordedValue, "Vote was not successfull");

                //refresh comment 
                x = db.GetComment(context.CommentID);

                Assert.AreEqual(ups, x.UpCount);
                Assert.AreEqual((downs + 1), x.DownCount);

                TestDirectVoteAccess(userName, Domain.Models.ContentType.Comment, context.CommentID, -1);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Down_NoCCP()
        {
            using (var db = new Voat.Data.Repository())
            {
                TestHelper.SetPrincipal("TestUser3", null); //Random User has no CCP

                var response = db.VoteComment(context.CommentID, -1, IpHash.CreateHash("127.0.0.1"));

                Assert.AreEqual(Status.Denied, response.Status);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Reset_Default()
        {
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                TestHelper.SetPrincipal("User100CCP", null);

                var response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"));
                
                //refresh comment 
                x = db.GetComment(context.CommentID);

                Assert.IsTrue(response.Success, response.ToString());
                Assert.AreEqual(1, response.RecordedValue, "Vote value incorrect");
                Assert.AreEqual((ups + 1), x.UpCount, "UpCount Off");

                //try to re-up vote, by default should revoke vote
                response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"));
                Assert.AreEqual(Status.Success, response.Status);
                Assert.AreEqual(0, response.RecordedValue, "Vote value incorrect");

                ////try to reset vote
                //response = repository.VoteComment(context.CommentID, 0);
                //Assert.IsTrue(response.Successfull);
                
                //refresh comment 
                x = db.GetComment(context.CommentID);

                Assert.AreEqual(ups, x.UpCount, "Final Up Count off");
                Assert.AreEqual(downs, x.DownCount, "Final Down Count off");
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Reset_NoRevoke()
        {
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                TestHelper.SetPrincipal("User100CCP", null);

                var response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"));

                //refresh comment 
                x = db.GetComment(context.CommentID);

                Assert.IsTrue(response.Success, response.ToString());
                Assert.AreEqual(1, response.RecordedValue, "Vote value incorrect");
                Assert.AreEqual((ups + 1), x.UpCount, "UpCount Off");


                //try to re-up vote, by default should revoke vote
                response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"), false);
                Assert.AreEqual(Status.Ignored, response.Status);
                Assert.AreEqual(1, response.RecordedValue, "Vote value incorrect");
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Up()
        {
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;
                string userName = "User50CCP";
                TestHelper.SetPrincipal(userName, null);

                var response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"));
                
                //refresh comment 
                x = db.GetComment(context.CommentID);

                Assert.IsTrue(response.Success, response.ToString());
                Assert.AreEqual(1, response.RecordedValue, "Vote value incorrect");
                Assert.AreEqual((ups + 1), x.UpCount, "UpCount Off");
                Assert.AreEqual(downs, x.DownCount, "DownCount Off");

                TestDirectVoteAccess(userName, Domain.Models.ContentType.Comment, context.CommentID, 1);

            }
        }

        private void TestDirectVoteAccess(string userName, Domain.Models.ContentType type, int id, int expectedValue)
        {
            using (var repo = new Voat.Data.Repository())
            {
                //Test direct vote access logic
                var result = repo.UserVoteStatus(userName, type, id);
                Assert.AreEqual(expectedValue, result, "Vote value incorrect direct pull");

                var voteResults = repo.UserVoteStatus(userName, type, new int[] { id });
                Assert.IsNotNull(voteResults, "Vote Results should not be null");
                Assert.IsTrue(voteResults.Any(), "Enumerable should have contents");

                var record = voteResults.FirstOrDefault(e => e.ID == id);
                Assert.IsNotNull(record, "Enumerable should have record matching ID");
                Assert.AreEqual(id, record.ID, "Record should match ID");
                Assert.AreEqual(expectedValue, record.Value, "Record should match vote value");

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCategory("Repository"), TestCategory("Repository.Vote")]
        public void EnsureInvalidVoteValueThrowsException_Com()
        {
            using (var db = new Voat.Data.Repository())
            {
                db.VoteComment(121, -2, IpHash.CreateHash("127.0.0.1"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCategory("Repository"), TestCategory("Repository.Vote")]
        public void EnsureInvalidVoteValueThrowsException_Sub()
        {
            using (var db = new Voat.Data.Repository())
            {
                db.VoteSubmission(1, 21, IpHash.CreateHash("127.0.0.1"));
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Down()
        {
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                string userName = "User500CCP";
                TestHelper.SetPrincipal(userName, null); //This user has one comment with 101 likes

                var response = db.VoteSubmission(context.SubmissionID, -1, IpHash.CreateHash("127.0.0.1"));

                Assert.AreEqual(Status.Success, response.Status, "Vote was not successfull");
                Assert.AreEqual(-1, response.RecordedValue, "Recorded value off");


                var expectedUpCount = ups;
                var expectedDownCount = downs + 1;
                Assert.AreEqual(expectedUpCount, response.Response.UpCount, "Response UpCount is off");
                Assert.AreEqual(expectedDownCount, response.Response.DownCount, "Response DownCount is off");
                //pull fresh data and compare
                x = db.GetSubmission(context.SubmissionID);
                Assert.AreEqual(expectedUpCount, x.UpCount, "Database UpCount is off");
                Assert.AreEqual(expectedDownCount, x.DownCount, "Database DownCount is off");

                TestDirectVoteAccess(userName, Domain.Models.ContentType.Submission, context.SubmissionID, -1);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Down_NoCCP()
        {
            using (var db = new Voat.Data.Repository())
            {
                TestHelper.SetPrincipal("TestUser3", null); //Random User has no CCP

                var response = db.VoteSubmission(context.SubmissionID, -1, IpHash.CreateHash("127.0.0.1"));

                Assert.AreEqual(Status.Denied, response.Status);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Reset_Default()
        {
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                TestHelper.SetPrincipal("User100CCP", null);

                var response = db.VoteSubmission(context.SubmissionID, 1, IpHash.CreateHash("127.0.0.1"));

                Assert.AreEqual(Status.Success, response.Status, "Vote was not successfull");
                Assert.AreEqual(1, response.RecordedValue, "Recorded value off");
                
                var expectedUpCount = ups + 1;
                var expectedDownCount = downs;
                Assert.AreEqual(expectedUpCount, response.Response.UpCount, "Response UpCount is off");
                Assert.AreEqual(expectedDownCount, response.Response.DownCount, "Response DownCount is off");
                //pull fresh data and compare
                x = db.GetSubmission(context.SubmissionID);
                Assert.AreEqual(expectedUpCount, x.UpCount, "Database UpCount is off");
                Assert.AreEqual(expectedDownCount, x.DownCount, "Database DownCount is off");

                //try to re-up vote
                response = db.VoteSubmission(context.SubmissionID, 1, IpHash.CreateHash("127.0.0.1"));
                Assert.IsTrue(response.Status == Status.Success);
                Assert.IsTrue(response.RecordedValue == 0);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Reset_NoRevoke()
        {
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                TestHelper.SetPrincipal("User100CCP", null);

                var response = db.VoteSubmission(context.SubmissionID, 1, IpHash.CreateHash("127.0.0.1"));

                Assert.AreEqual(Status.Success, response.Status, "Vote was not successfull");
                Assert.AreEqual(1, response.RecordedValue, "Recorded value off");

                var expectedUpCount = ups + 1;
                var expectedDownCount = downs;
                Assert.AreEqual(expectedUpCount, response.Response.UpCount, "Response UpCount is off");
                Assert.AreEqual(expectedDownCount, response.Response.DownCount, "Response DownCount is off");
                //pull fresh data and compare
                x = db.GetSubmission(context.SubmissionID);
                Assert.AreEqual(expectedUpCount, x.UpCount, "Database UpCount is off");
                Assert.AreEqual(expectedDownCount, x.DownCount, "Database DownCount is off");

                //try to re-up vote
                response = db.VoteSubmission(context.SubmissionID, 1, IpHash.CreateHash("127.0.0.1"), false);
                Assert.IsTrue(response.Status == Status.Ignored); //setting tells voting to ignore if submitted vote is current vote
                Assert.IsTrue(response.RecordedValue == 1); //should still be an upvote
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Up()
        {
            using (var db = new Voat.Data.Repository())
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                string userName = "User50CCP";
                TestHelper.SetPrincipal(userName, null);

                var response = db.VoteSubmission(context.SubmissionID, 1, IpHash.CreateHash("127.0.0.1"));

                var expectedUpCount = ups + 1;
                var expectedDownCount = downs;
                Assert.AreEqual(expectedUpCount, response.Response.UpCount, "Response UpCount is off");
                Assert.AreEqual(expectedDownCount, response.Response.DownCount, "Response DownCount is off");
                //pull fresh data and compare
                x = db.GetSubmission(context.SubmissionID);
                Assert.AreEqual(expectedUpCount, x.UpCount, "Database UpCount is off");
                Assert.AreEqual(expectedDownCount, x.DownCount, "Database DownCount is off");

                TestDirectVoteAccess(userName, Domain.Models.ContentType.Submission, context.SubmissionID, 1);
            }
        }

    }
}
