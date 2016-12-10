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
        public void DownvoteComment()
        {
            TestHelper.SetPrincipal("User500CCP");
            bool voteEventReceived = false;
            EventNotification.Instance.OnVoteReceived += (s, e) => {
              voteEventReceived = e.TargetUserName == "unit" && e.SendingUserName == "User500CCP" && e.ChangeValue == -1 && e.ReferenceType == Domain.Models.ContentType.Comment && e.ReferenceID == 1;
            };
            var cmd = new CommentVoteCommand(1, -1, IpHash.CreateHash("127.0.0.1"));

            var c = cmd.Execute().Result;
            Assert.IsTrue(c.Success);
            Assert.IsNotNull(c.Response);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(1);
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
            TestHelper.SetPrincipal("UnitTestUser45");
            var cmd = new SubmissionVoteCommand(1, 1, IpHash.CreateHash("1.1.1.1")); 
            var c = cmd.Execute().Result;
            Assert.IsNotNull(c, "Response is null");
            Assert.IsTrue(c.Success, c.Message);
            Assert.IsNotNull(c.Response);

            TestHelper.SetPrincipal("UnitTestUser46");
            cmd = new SubmissionVoteCommand(1, 1, IpHash.CreateHash("1.1.1.1"));
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
            TestHelper.SetPrincipal("User0CCP");

            var cmd = new CommentVoteCommand(5, -1, IpHash.CreateHash("127.0.0.1")); //SubmissionID: 3 is in MinCCP sub

            var c = cmd.Execute().Result;
            Assert.IsFalse(c.Success);
            Assert.IsNull(c.Response);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidVoteValue_Comment_Low()
        {
            TestHelper.SetPrincipal("unit");
            var cmd = new CommentVoteCommand(1, -2, IpHash.CreateHash("127.0.0.1"));
            var c = cmd.Execute().Result;
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        public void UpvoteComment()
        {
            TestHelper.SetPrincipal("User50CCP");
            var cmd = new CommentVoteCommand(1, 1, IpHash.CreateHash("127.0.0.2"));

            var c = cmd.Execute().Result;
            Assert.IsNotNull(c, "Response is null");
            Assert.IsTrue(c.Success, c.Message);
            Assert.IsNotNull(c.Response);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(1);
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
            TestHelper.SetPrincipal("User500CCP");
            bool voteEventReceived = false;

            EventNotification.Instance.OnVoteReceived += (s, e) => {
                voteEventReceived = 
                    e.TargetUserName == "anon" 
                    && e.SendingUserName == "User500CCP" 
                    && e.ChangeValue == -1 
                    && e.ReferenceType == Domain.Models.ContentType.Submission 
                    && e.ReferenceID == 1;
            };
            var cmd = new SubmissionVoteCommand(1, -1, IpHash.CreateHash("127.0.0.1"));

            var c = cmd.Execute().Result;
            Assert.IsTrue(c.Success, c.Message);
            Assert.IsNotNull(c.Response, "Expecting payload");

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetSubmission(1);
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
        public void UpvoteSubmission()
        {
            TestHelper.SetPrincipal("User50CCP");
            bool voteEventReceived = false;

            EventNotification.Instance.OnVoteReceived += (s, e) => {
                voteEventReceived =
                    e.TargetUserName == "anon"
                    && e.SendingUserName == "User50CCP"
                    && e.ChangeValue == 1
                    && e.ReferenceType == Domain.Models.ContentType.Submission
                    && e.ReferenceID == 1;
            };
            var cmd = new SubmissionVoteCommand(1, 1, IpHash.CreateHash("127.0.0.2"));

            var c = cmd.Execute().Result;
            Assert.IsNotNull(c, "Response is null");
            Assert.IsTrue(c.Success, c.Message);
            Assert.IsNotNull(c.Response);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetSubmission(1);
                Assert.IsNotNull(comment, "Couldn't find submission in db");
                Assert.AreEqual(comment.UpCount, c.Response.UpCount);
                Assert.AreEqual(comment.DownCount, c.Response.DownCount);
            }
            Assert.IsTrue(voteEventReceived, "VoteEvent not have the expected values");
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Submission.Vote")]
        public void DownvoteSubmission_MinCCP()
        {
            TestHelper.SetPrincipal("User0CCP");

            var cmd = new SubmissionVoteCommand(3, -1, IpHash.CreateHash("127.0.0.1")); //SubmissionID: 3 is in MinCCP sub

            var c = cmd.Execute().Result;
            Assert.IsFalse(c.Success);
            Assert.IsNull(c.Response);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidVoteValue_Submission_High()
        {
            TestHelper.SetPrincipal("unit");

            var cmd = new SubmissionVoteCommand(1, 2, IpHash.CreateHash("127.0.0.1"));

            var c = cmd.Execute().Result;
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidVoteValue_Submission_Low()
        {
            TestHelper.SetPrincipal("unit");

            var cmd = new CommentVoteCommand(1, -2, IpHash.CreateHash("127.0.0.1"));

            var c = cmd.Execute().Result;
        }

       

        #endregion Submission Voat Commands
    }
}
