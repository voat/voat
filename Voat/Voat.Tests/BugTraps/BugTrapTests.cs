using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Data;
using Voat.Tests.Repository;
using Voat.Domain.Command;
using Voat.Models;
using Voat.Controllers;
using System.Threading;
using Voat.Utilities;
using System.Diagnostics;
using Moq;
using System.Web.Mvc;

namespace Voat.Tests.BugTraps
{
    [TestClass]
    public class BugTrapTests
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
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User500CCP", "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                var cmd = new SubmissionVoteCommand(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                Interlocked.Increment(ref exCount);
                return await cmd.Execute();//.Result;
            });

            Func<Task<VoteResponse>> vote2 = new Func<Task<VoteResponse>>(async () =>
            {
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User100CCP", "Bearer"), null);
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
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User500CCP", "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository())
                {
                    return repo.VoteSubmission(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                }
            });
            Func<VoteResponse> vote2 = new Func<VoteResponse>(() =>
            {
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User100CCP", "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository())
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
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User500CCP", "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                var cmd = new SubmissionVoteCommand(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                Interlocked.Increment(ref exCount);
                return await cmd.Execute();//.Result;
            });
            Func<Task<VoteResponse>> vote2 = new Func<Task<VoteResponse>>(async () =>
            {
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User100CCP", "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                var cmd = new SubmissionVoteCommand(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
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
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User500CCP", "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository())
                {
                    return repo.VoteSubmission(submissionID, 1, IpHash.CreateHash("127.0.0.1"));
                }
            });
            Func<VoteResponse> vote2 = new Func<VoteResponse>(() =>
            {
                var principle = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity("User100CCP", "Bearer"), null);
                System.Threading.Thread.CurrentPrincipal = principle;
                Interlocked.Increment(ref exCount);
                using (var repo = new Voat.Data.Repository())
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
