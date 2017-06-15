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
using System.Linq;
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;
using Voat.Utilities;

namespace Voat.Tests.Repository
{
    [TestClass]
    public class RepositoryTests_Voting : BaseUnitTest
    {
        public TestHelper.ContentContext context = null;

        [TestInitialize]
        public override void TestInitialize()
        {
            context = TestHelper.ContentContext.Create(true);
        }


        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public async Task Comment_Down()
        {
            string userName = USERNAMES.User500CCP;
            var user = TestHelper.SetPrincipal(userName, null); //This user has one comment with 101 likes

            using (var db = new Voat.Data.Repository(user))
            {
               var x = await db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                var response = db.VoteComment(context.CommentID, -1, IpHash.CreateHash("127.0.0.1"));
                Assert.IsTrue(response.Success, response.Message);
                Assert.AreEqual(-1, response.RecordedValue, "Vote was not successfull");

                //refresh comment 
                x = await db.GetComment(context.CommentID);

                Assert.AreEqual(ups, x.UpCount);
                Assert.AreEqual((downs + 1), x.DownCount);

                TestDirectVoteAccess(userName, Domain.Models.ContentType.Comment, context.CommentID, -1);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public void Comment_Down_NoCCP()
        {
            var user = TestHelper.SetPrincipal("TestUser03", null); //Random User has no CCP

            using (var db = new Voat.Data.Repository(user))
            {
                var response = db.VoteComment(context.CommentID, -1, IpHash.CreateHash("127.0.0.1"));

                Assert.AreEqual(Status.Denied, response.Status);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public async Task Comment_Reset_Default()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User100CCP, null);

            using (var db = new Voat.Data.Repository(user))
            {
                var x = await db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                var response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"));
                
                //refresh comment 
                x = await db.GetComment(context.CommentID);

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
                x = await db.GetComment(context.CommentID);

                Assert.AreEqual(ups, x.UpCount, "Final Up Count off");
                Assert.AreEqual(downs, x.DownCount, "Final Down Count off");
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Comment")]
        public async Task Comment_Reset_NoRevoke()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User100CCP, null);

            using (var db = new Voat.Data.Repository(user))
            {
                var x = await db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                var response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"));

                //refresh comment 
                x = await db.GetComment(context.CommentID);

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
        public async Task Comment_Up()
        {
            string userName = USERNAMES.User50CCP;
            var user = TestHelper.SetPrincipal(userName, null);

            using (var db = new Voat.Data.Repository(user))
            {
                var x = await db.GetComment(context.CommentID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                var response = db.VoteComment(context.CommentID, 1, IpHash.CreateHash("127.0.0.1"));
                
                //refresh comment 
                x = await db.GetComment(context.CommentID);

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
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCategory("Repository"), TestCategory("Repository.Vote")]
        public void EnsureInvalidVoteValueThrowsException_Com()
        {
            string userName = USERNAMES.User500CCP;
            var user = TestHelper.SetPrincipal(userName, null); //This user has one comment with 101 likes

            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                using (var db = new Voat.Data.Repository(user))
                {
                    db.VoteComment(121, -2, IpHash.CreateHash("127.0.0.1"));
                }
            });
        }

        [TestMethod]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCategory("Repository"), TestCategory("Repository.Vote")]
        public void EnsureInvalidVoteValueThrowsException_Sub()
        {
            string userName = USERNAMES.User500CCP;
            var user = TestHelper.SetPrincipal(userName, null); //This user has one comment with 101 likes

            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                using (var db = new Voat.Data.Repository(user))
                {
                    db.VoteSubmission(1, 21, IpHash.CreateHash("127.0.0.1"));
                }
            });
            
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Down()
        {
            string userName = USERNAMES.User500CCP;
            var user = TestHelper.SetPrincipal(userName, null); //This user has one comment with 101 likes

            using (var db = new Voat.Data.Repository(user))
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;

                var response = db.VoteSubmission(context.SubmissionID, -1, IpHash.CreateHash("127.0.0.1"));

                Assert.AreEqual(Status.Success, response.Status, response.Message);
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
            var user = TestHelper.SetPrincipal("TestUser03", null); //Random User has no CCP

            using (var db = new Voat.Data.Repository(user))
            {
                var response = db.VoteSubmission(context.SubmissionID, -1, IpHash.CreateHash("127.0.0.1"));
                Assert.AreEqual(Status.Denied, response.Status);
            }
        }

        [TestMethod]
        [TestCategory("Repository"), TestCategory("Repository.Vote"), TestCategory("Repository.Vote.Submission")]
        public void Submission_Reset_Default()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User100CCP, null);

            using (var db = new Voat.Data.Repository(user))
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;

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
            var user = TestHelper.SetPrincipal(USERNAMES.User100CCP, null);

            using (var db = new Voat.Data.Repository(user))
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;
                
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
            string userName = USERNAMES.User50CCP;
            var user = TestHelper.SetPrincipal(userName, null);
            using (var db = new Voat.Data.Repository(user))
            {
                var x = db.GetSubmission(context.SubmissionID);
                var ups = x.UpCount;
                var downs = x.DownCount;

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
