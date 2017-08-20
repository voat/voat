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
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Models;
using System.Threading;
using Voat.Utilities;
using Voat.Common;
using Voat.Tests.Infrastructure;
using Voat.Data.Models;
using System.Linq;

namespace Voat.Tests.BugTraps
{
    [TestClass]
    public class BugTrapTests : BaseUnitTest
    {
        private int count = 31;
        private int submissionID = 1;

        [TestMethod]
        [TestCategory("Bug"), TestCategory("Voting")]
        public void Bug_Trap_Spam_Votes_VoteCommand()
        {

            int submissionID = 1;
            Submission beforesubmission = GetSubmission();

            int exCount = 0;
            Func<Task<VoteResponse>> vote1 = new Func<Task<VoteResponse>>(async () =>
            {
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(USERNAMES.User500CCP, "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                var cmd = new SubmissionVoteCommand(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                Interlocked.Increment(ref exCount);
                return await cmd.Execute();//.Result;
            });

            Func<Task<VoteResponse>> vote2 = new Func<Task<VoteResponse>>(async () =>
            {
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(USERNAMES.User100CCP, "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                var cmd = new SubmissionVoteCommand(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                Interlocked.Increment(ref exCount);
                return await cmd.Execute();//.Result;
            });

            //exCount = -2;
            //var x = vote1().Result;
            //var y = vote2().Result;


            var tasks = new List<Task<VoteResponse>>();
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    tasks.Add(Task.Run(vote1));
                }
                else
                {
                    tasks.Add(Task.Run(vote2));
                }
            }

            Task.WaitAll(tasks.ToArray());

            Submission aftersubmission = GetSubmission();

            Assert.AreEqual(count, exCount, "Execution count is off");
            AssertData(beforesubmission, aftersubmission);
        }
      
        [TestMethod]
        [TestCategory("Bug"), TestCategory("Voting")]
        public void Bug_Trap_Spam_Votes_Repository()
        {
            Submission beforesubmission = GetSubmission();
            int exCount = 0;
            Func<VoteResponse> vote1 = new Func<VoteResponse>(() =>
            {
                var principal = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(USERNAMES.User500CCP, "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principal;
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository(principal))
                {
                    return repo.VoteSubmission(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                }
            });
            Func<VoteResponse> vote2 = new Func<VoteResponse>(() =>
            {
                var principal = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(USERNAMES.User100CCP, "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principal;
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository(principal))
                {
                    return repo.VoteSubmission(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                }
            });

            var tasks = new List<Task<VoteResponse>>();
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    tasks.Add(Task.Run(vote1));
                }
                else
                {
                    tasks.Add(Task.Run(vote2));
                }
            }

            Task.WaitAll(tasks.ToArray());

            Submission aftersubmission = GetSubmission();
            Assert.AreEqual(count, exCount, "Execution count is off");
            AssertData(beforesubmission, aftersubmission);
        }


        [TestMethod]
        [TestCategory("Bug"), TestCategory("User.Delete"), TestCategory("Process")]
        public async Task Bug_Trap_Positive_ContributionPoints_Removed()
        {
            //Test that when a user deletes comments and submissions with positive points, that the points are reset
            var altList = new[] { "UnitTestUser10", "UnitTestUser11", "UnitTestUser12", "UnitTestUser13", "UnitTestUser14", "UnitTestUser15" };
            var primaryUser = "UnitTestUser20";
            var currentUser = TestHelper.SetPrincipal(primaryUser);
            var cmdSubmission = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Title = "Test Positive SCP Removed upon Delete", Content = "Does this get removed?", Subverse = "unit" }).SetUserContext(currentUser);
            var subResponse = await cmdSubmission.Execute();
            VoatAssert.IsValid(subResponse);
            var submissionID = subResponse.Response.ID;

            var cmdComment = new CreateCommentCommand(submissionID, null, "This is my manipulation comment. Upvote. Go.").SetUserContext(currentUser);
            var commentResponse = await cmdComment.Execute();
            VoatAssert.IsValid(commentResponse);
            var commentID = commentResponse.Response.ID;

            var vote = new Func<int, Domain.Models.ContentType, string[], Task>(async (id, contentType, users) => {

                foreach (string user in users)
                {
                    var userIdentity = TestHelper.SetPrincipal(user);
                    switch (contentType)
                    {
                        case Domain.Models.ContentType.Comment:
                            
                            var c = new CommentVoteCommand(id, 1, Guid.NewGuid().ToString()).SetUserContext(userIdentity);
                            var cr = await c.Execute();
                            VoatAssert.IsValid(cr);

                            break;
                        case Domain.Models.ContentType.Submission:

                            var s = new SubmissionVoteCommand(id, 1, Guid.NewGuid().ToString()).SetUserContext(userIdentity);
                            var sr = await s.Execute();
                            VoatAssert.IsValid(sr);
                            break;
                    }
                }
            });

            await vote(commentID, Domain.Models.ContentType.Comment, altList);
            var deleteComment = new DeleteCommentCommand(commentID).SetUserContext(currentUser);
            var deleteCommentResponse = await deleteComment.Execute();
            VoatAssert.IsValid(deleteCommentResponse);
            //verify ups where reset 
            using (var context = new Voat.Data.Models.VoatDataContext())
            {
                var votes = context.CommentVoteTracker.Where(x => x.CommentID == commentID);
                Assert.AreEqual(altList.Length, votes.Count());
                var anyInvalid = votes.Any(x => x.VoteValue != 0);
                Assert.IsFalse(anyInvalid, "Found comment votes with a non-zero vote value");
            }


            await vote(submissionID, Domain.Models.ContentType.Submission, altList);
            var deleteSubmission = new DeleteSubmissionCommand(submissionID).SetUserContext(currentUser);
            var deleteSubmissionResponse = await deleteSubmission.Execute();
            VoatAssert.IsValid(deleteSubmissionResponse);
            //verify ups where reset 
            using (var context = new Voat.Data.Models.VoatDataContext())
            {
                var votes = context.SubmissionVoteTracker.Where(x => x.SubmissionID == submissionID);
                Assert.AreEqual(altList.Length + 1, votes.Count()); //author has a vote
                var anyInvalid = votes.Any(x => x.VoteValue != 0);
                Assert.IsFalse(anyInvalid, "Found submission votes with a non-zero vote value");
            }
        }


        #region Dups

        [TestMethod]
        [TestCategory("Bug")]
        public void Bug_Trap_Spam_Votes_VoteCommand_2()
        {

            int submissionID = 1;
            Submission beforesubmission = GetSubmission();

            int exCount = 0;
            Func<Task<VoteResponse>> vote1 = new Func<Task<VoteResponse>>(async () =>
            {
                var user = TestHelper.SetPrincipal(USERNAMES.User500CCP);
                var cmd = new SubmissionVoteCommand(submissionID, 1, IpHash.CreateHash("127.0.0.1")).SetUserContext(user);
                Interlocked.Increment(ref exCount);
                return await cmd.Execute();//.Result;
            });
            Func<Task<VoteResponse>> vote2 = new Func<Task<VoteResponse>>(async () =>
            {
                var user = TestHelper.SetPrincipal(USERNAMES.User100CCP);
                var cmd = new SubmissionVoteCommand(submissionID, 1, IpHash.CreateHash("127.0.0.1")).SetUserContext(user); ;
                Interlocked.Increment(ref exCount);
                return await cmd.Execute();//.Result;
            });

            var tasks = new List<Task<VoteResponse>>();
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    tasks.Add(Task.Run(vote1));
                }
                else
                {
                    tasks.Add(Task.Run(vote2));
                }
            }

            Task.WaitAll(tasks.ToArray());

            Submission aftersubmission = GetSubmission();

            Assert.AreEqual(count, exCount, "Execution count is off");
            AssertData(beforesubmission, aftersubmission);
        }

        [TestMethod]
        [TestCategory("Bug")]
        public void Bug_Trap_Spam_Votes_Repository_2()
        {
            Submission beforesubmission = GetSubmission();
            int exCount = 0;
            Func<VoteResponse> vote1 = new Func<VoteResponse>(() =>
            {
                var user = TestHelper.SetPrincipal(USERNAMES.User500CCP);
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository(user))
                {
                    return repo.VoteSubmission(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                }
            });
            Func<VoteResponse> vote2 = new Func<VoteResponse>(() =>
            {
                var user = TestHelper.SetPrincipal(USERNAMES.User100CCP);
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository(user))
                {
                    return repo.VoteSubmission(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                }
            });

            var tasks = new List<Task<VoteResponse>>();
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    tasks.Add(Task.Run(vote1));
                }
                else
                {
                    tasks.Add(Task.Run(vote2));
                }
            }

            Task.WaitAll(tasks.ToArray());

            Submission aftersubmission = GetSubmission();
            Assert.AreEqual(count, exCount, "Execution count is off");
            AssertData(beforesubmission, aftersubmission);
        }
        #endregion

        private Submission GetSubmission()
        {
            using (var repo = new Voat.Data.Repository())
            {
                return repo.GetSubmission(submissionID);
            }
        }
        private void AssertData(Submission beforeSubmission, Submission afterSubmission)
        {
            long upCountDiff = beforeSubmission.UpCount - afterSubmission.UpCount;
            long downCountDiff = beforeSubmission.DownCount - afterSubmission.DownCount;

            Assert.IsTrue(Math.Abs(upCountDiff + downCountDiff) <= 2, String.Format("Difference detected: UpCount Diff: {0}, Down Count Diff: {1}", upCountDiff, downCountDiff));
            Assert.IsTrue(Math.Abs(upCountDiff) <= 1, String.Format("Before {0} threads: UpCount: {1}, Afterwards: {2}", count, beforeSubmission.UpCount, afterSubmission.UpCount));
            Assert.IsTrue(Math.Abs(downCountDiff) <= 1, String.Format("Before {0} threads: DownCount: {1}, Afterwards: {2}", count, beforeSubmission.DownCount, afterSubmission.DownCount));
        }
    }
}
