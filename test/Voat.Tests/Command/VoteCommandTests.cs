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
using Voat.Common;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Notifications;
using Voat.Tests.Infrastructure;
using Voat.Tests.Repository;
using Voat.Utilities;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class VoteCommandTests : BaseUnitTest
    {
        #region Comment Vote Commands

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        public async Task DownvoteComment()
        {
            var userName = USERNAMES.User500CCP;
            var user = TestHelper.SetPrincipal(userName);
            bool voteEventReceived = false;
            EventNotification.Instance.OnVoteReceived += (s, e) => {
              voteEventReceived = e.TargetUserName == USERNAMES.Unit && e.SendingUserName == userName && e.ChangeValue == -1 && e.ReferenceType == Domain.Models.ContentType.Comment && e.ReferenceID == 1;
            };
            var cmd = new CommentVoteCommand(1, -1, IpHash.CreateHash("127.0.0.1")).SetUserContext(user);

            var c = cmd.Execute().Result;
            VoatAssert.IsValid(c);

            //verify in db
            using (var db = new Voat.Data.Repository(user))
            {
                var comment = await db.GetComment(1);
                Assert.IsNotNull(comment, "Couldn't find comment in db");
                Assert.AreEqual(comment.UpCount, c.Response.UpCount);
                Assert.AreEqual(comment.DownCount, c.Response.DownCount);
            }
            Assert.IsTrue(voteEventReceived, "VoteEvent not have the expected values");

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Submission.Vote")]
        public void UpvoteSubmission_DeniesSameIP()
        {
            var user = TestHelper.SetPrincipal("UnitTestUser45");
            var cmd = new SubmissionVoteCommand(1, 1, IpHash.CreateHash("1.1.1.1")).SetUserContext(user); 
            var c = cmd.Execute().Result;
            VoatAssert.IsValid(c);

            user = TestHelper.SetPrincipal("UnitTestUser46");
            cmd = new SubmissionVoteCommand(1, 1, IpHash.CreateHash("1.1.1.1")).SetUserContext(user);
            c = cmd.Execute().Result;
            Assert.IsNotNull(c, "Response is null");
            Assert.IsFalse(c.Success, c.Message);
            Assert.IsNull(c.Response);


        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Submission.Vote")]
        public void DownvoteComment_MinCCP()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User0CCP);

            var cmd = new CommentVoteCommand(5, -1, IpHash.CreateHash("127.0.0.1")).SetUserContext(user); //SubmissionID: 3 is in MinCCP sub

            var c = cmd.Execute().Result;
            Assert.IsFalse(c.Success);
            Assert.IsNull(c.Response);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        public void InvalidVoteValue_Comment_Low()
        {
            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                var user = TestHelper.SetPrincipal(USERNAMES.Unit);
                var cmd = new CommentVoteCommand(1, -2, IpHash.CreateHash("127.0.0.1")).SetUserContext(user);
                var c = cmd.Execute().Result;
            });
           
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        public async Task UpvoteComment()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User50CCP);
            var cmd = new CommentVoteCommand(1, 1, IpHash.CreateHash("127.0.0.2")).SetUserContext(user);

            var c = cmd.Execute().Result;
            VoatAssert.IsValid(c);

            //verify in db
            using (var db = new Voat.Data.Repository(user))
            {
                var comment = await db.GetComment(1);
                Assert.IsNotNull(comment, "Couldn't find comment in db");
                Assert.AreEqual(comment.UpCount, c.Response.UpCount);
                Assert.AreEqual(comment.DownCount, c.Response.DownCount);
            }
        }

        #endregion Comment Vote Commands

        #region Submission Voat Commands

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Submission.Vote")]
        public void DownvoteSubmission()
        {
            var submissionUser = "UnitTestUser47";
            var newSubmission = TestHelper.ContentCreation.CreateSubmission(submissionUser, new Domain.Models.UserSubmission() { Title = "This is what I think about you guys", Subverse = SUBVERSES.Unit });

            var userName = USERNAMES.User500CCP;
            var user = TestHelper.SetPrincipal(userName);
            bool voteEventReceived = false;

            EventNotification.Instance.OnVoteReceived += (s, e) => {
                voteEventReceived = 
                    e.TargetUserName == submissionUser
                    && e.SendingUserName == userName
                    && e.ChangeValue == -1 
                    && e.ReferenceType == Domain.Models.ContentType.Submission 
                    && e.ReferenceID == newSubmission.ID;
            };
            var cmd = new SubmissionVoteCommand(newSubmission.ID, -1, IpHash.CreateHash("127.0.0.100")).SetUserContext(user);

            var c = cmd.Execute().Result;
            VoatAssert.IsValid(c);

            //verify in db
            using (var db = new Voat.Data.Repository(user))
            {
                var submissionFromRepo = db.GetSubmission(newSubmission.ID);
                Assert.IsNotNull(submissionFromRepo, "Couldn't find comment in db");
                Assert.AreEqual(submissionFromRepo.UpCount, c.Response.UpCount);
                Assert.AreEqual(submissionFromRepo.DownCount, c.Response.DownCount);
            }
            Assert.IsTrue(voteEventReceived, "VoteEvent not have the expected values");

            //Verify Submission pull has correct vote value recorded in output for current user
            var q = new QuerySubmission(newSubmission.ID, true).SetUserContext(user);
            var submission = q.Execute();
            Assert.IsNotNull(submission);
            Assert.AreEqual(c.RecordedValue, submission.Vote);

            //Verify non-logged in user has correct vote value
            TestHelper.SetPrincipal(null);
            q = new QuerySubmission(1, true);
            submission = q.Execute();
            Assert.IsNotNull(submission);
            Assert.AreEqual(null, submission.Vote);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Submission.Vote")]
        public void UpvoteSubmission()
        {
            var submissionUser = "UnitTestUser48";
            var newSubmission = TestHelper.ContentCreation.CreateSubmission(submissionUser, new Domain.Models.UserSubmission() { Title = "This is what I think about you guys", Subverse = SUBVERSES.Unit });

            var user = TestHelper.SetPrincipal(USERNAMES.User50CCP);
            bool voteEventReceived = false;

            EventNotification.Instance.OnVoteReceived += (s, e) => {
                voteEventReceived =
                    e.TargetUserName == submissionUser
                    && e.SendingUserName == USERNAMES.User50CCP
                    && e.ChangeValue == 1
                    && e.ReferenceType == Domain.Models.ContentType.Submission
                    && e.ReferenceID == newSubmission.ID;
            };
            var cmd = new SubmissionVoteCommand(newSubmission.ID, 1, IpHash.CreateHash("127.0.0.2")).SetUserContext(user);

            var c = cmd.Execute().Result;
            VoatAssert.IsValid(c);

            //verify in db
            using (var db = new Voat.Data.Repository(user))
            {
                var comment = db.GetSubmission(newSubmission.ID);
                Assert.IsNotNull(comment, "Couldn't find submission in db");
                Assert.AreEqual(comment.UpCount, c.Response.UpCount);
                Assert.AreEqual(comment.DownCount, c.Response.DownCount);
            }
            Assert.IsTrue(voteEventReceived, "VoteEvent not have the expected values");

            //Verify Submission pull has correct vote value recorded in output for current user
            var q = new QuerySubmission(newSubmission.ID, true).SetUserContext(user);
            var submission = q.Execute();
            Assert.IsNotNull(submission);
            Assert.AreEqual(c.RecordedValue, submission.Vote);

            //Verify non-logged in user has correct vote value
            TestHelper.SetPrincipal(null);
            q = new QuerySubmission(newSubmission.ID, true);
            submission = q.Execute();
            Assert.IsNotNull(submission);
            Assert.AreEqual(null, submission.Vote);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Submission.Vote")]
        public void DownvoteSubmission_MinCCP()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User0CCP);

            var cmd = new SubmissionVoteCommand(3, -1, IpHash.CreateHash("127.0.0.1")).SetUserContext(user); //SubmissionID: 3 is in MinCCP sub

            var c = cmd.Execute().Result;
            Assert.IsFalse(c.Success);
            Assert.IsNull(c.Response);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        public void InvalidVoteValue_Submission_High()
        {

            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                var user = TestHelper.SetPrincipal(USERNAMES.Unit);

                var cmd = new SubmissionVoteCommand(1, 2, IpHash.CreateHash("127.0.0.1")).SetUserContext(user);

                var c = cmd.Execute().Result;
            });

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        public void InvalidVoteValue_Submission_Low()
        {

            VoatAssert.Throws<ArgumentOutOfRangeException>(() => {
                var user = TestHelper.SetPrincipal(USERNAMES.Unit);

                var cmd = new CommentVoteCommand(1, -2, IpHash.CreateHash("127.0.0.1")).SetUserContext(user);

                var c = cmd.Execute().Result;
            });

        }

       

        #endregion Submission Voat Commands
    }
}
