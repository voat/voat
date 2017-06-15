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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;
using Voat.Utilities;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class VoteValueCommandTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        [TestCategory("Command.Comment.Vote.VoteValue")]
        public async Task Vote_NonRestricted()
        {
            //Anon voting should not count towards target
            await VerifyVoteStatus("TestUser25", SUBVERSES.Unit, Domain.Models.ContentType.Submission, 1, 1);
            await VerifyVoteStatus("TestUser25", SUBVERSES.Unit, Domain.Models.ContentType.Submission, -1, -1);

            await VerifyVoteStatus("TestUser26", SUBVERSES.Unit, Domain.Models.ContentType.Comment, 1, 1);
            await VerifyVoteStatus("TestUser26", SUBVERSES.Unit, Domain.Models.ContentType.Comment, -1, -1);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        [TestCategory("Command.Comment.Vote.VoteValue")]
        public async Task Vote_Anon()
        {
            //Anon voting should not count towards target
            await VerifyVoteStatus("TestUser21", SUBVERSES.Anon, Domain.Models.ContentType.Submission, 1, 0);
            await VerifyVoteStatus("TestUser21", SUBVERSES.Anon, Domain.Models.ContentType.Submission, -1, 0);

            await VerifyVoteStatus("TestUser11", SUBVERSES.Anon, Domain.Models.ContentType.Comment, 1, 0);
            await VerifyVoteStatus("TestUser11", SUBVERSES.Anon, Domain.Models.ContentType.Comment, -1, 0);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        [TestCategory("Command.Comment.Vote.VoteValue")]
        public async Task Vote_Private()
        {
            //Anon voting should not count towards target
            await VerifyVoteStatus("TestUser22", SUBVERSES.Private, Domain.Models.ContentType.Submission, 1, 0);
            await VerifyVoteStatus("TestUser22", SUBVERSES.Private, Domain.Models.ContentType.Submission, -1, 0);

            await VerifyVoteStatus("TestUser23", SUBVERSES.Private, Domain.Models.ContentType.Comment, 1, 0);
            await VerifyVoteStatus("TestUser23", SUBVERSES.Private, Domain.Models.ContentType.Comment, -1, 0);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Command.Vote")]
        [TestCategory("Command.Comment.Vote")]
        [TestCategory("Command.Comment.Vote.VoteValue")]
        public async Task Vote_MinCCP()
        {
            //Anon voting should not count towards target
            await VerifyVoteStatus("TestUser24", SUBVERSES.MinCCP, Domain.Models.ContentType.Submission, 1, 0);

            await VerifyVoteStatus("TestUser25", SUBVERSES.MinCCP, Domain.Models.ContentType.Comment, 1, 0);
        }
        private async Task VerifyVoteStatus(string userToPost, string subverse, Domain.Models.ContentType contentType, int voteStatus, int voteValue)
        {
            int id = 0;
            string userName = "";
            //Create submission
            var user = TestHelper.SetPrincipal(userToPost);
            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = subverse, Title = "VerifyVoteStatus Test Submission in " + subverse }).SetUserContext(user);
            var response = cmd.Execute().Result;

            VoatAssert.IsValid(response, Status.Success);
            //Assert.AreEqual(Status.Success, response.Status, response.Message);
            var submission = response.Response;

            //voting username
            userName = USERNAMES.User100CCP;
            if (contentType == Domain.Models.ContentType.Submission)
            {
                id = submission.ID;
                user = TestHelper.SetPrincipal(userName);
                var voteSubmissionCommand = new SubmissionVoteCommand(id, voteStatus, Guid.NewGuid().ToString()).SetUserContext(user);
                var voteSubmissionResponse = await voteSubmissionCommand.Execute();

                VoatAssert.IsValid(voteSubmissionResponse);
                Assert.IsNotNull(voteSubmissionResponse, "Expecting non-null submission vote command");

                //verify in db
                using (var db = new VoatDataContext())
                {
                    var record = db.SubmissionVoteTracker.Where(x => x.SubmissionID == id && x.UserName == userName).FirstOrDefault();
                    Assert.IsNotNull(record, "Did not find Vote Record in database");
                    Assert.AreEqual(voteStatus, record.VoteStatus);
                    Assert.AreEqual(voteValue, record.VoteValue);
                }

            }
            else if (contentType == Domain.Models.ContentType.Comment)
            {
                //Create comment 
                var cmdComment = new CreateCommentCommand(submission.ID, null, $"VerifyVoteStatus Test Submission in {subverse} - {Guid.NewGuid().ToString()}").SetUserContext(user);
                var responseComment = await cmdComment.Execute();
                VoatAssert.IsValid(responseComment);

                id = responseComment.Response.ID;

                user = TestHelper.SetPrincipal(userName);
                var voteCommentCommand = new CommentVoteCommand(id, voteStatus, Guid.NewGuid().ToString()).SetUserContext(user);
                var voteCommentResponse = await voteCommentCommand.Execute();
                Assert.IsNotNull(voteCommentResponse, "Expecting non-null submission vote command");

                //verify in db
                using (var db = new VoatDataContext())
                {
                    var record = db.CommentVoteTracker.Where(x => x.CommentID == id && x.UserName == userName).FirstOrDefault();
                    Assert.IsNotNull(record, "Did not find Vote Record in database");
                    Assert.AreEqual(voteStatus, record.VoteStatus);
                    Assert.AreEqual(voteValue, record.VoteValue);
                }
            }
        }
    }
}
