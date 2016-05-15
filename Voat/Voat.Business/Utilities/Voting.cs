/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;
using System.Data.Entity;
using System.Linq;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
//using Microsoft.AspNet.SignalR;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class Voting
    {
        private static LockStore _lockStore = new LockStore();

        // returns -1:downvoted, 1:upvoted, 0:not voted
        public static int CheckIfVoted(string userToCheck, int submissionID)
        {
            using (var db = new voatEntities())
            {
                var checkResult = db.SubmissionVoteTrackers.Where(u => u.UserName == userToCheck && u.SubmissionID == submissionID)
                        .AsNoTracking()
                        .FirstOrDefault();

                int intCheckResult = checkResult != null ? checkResult.VoteStatus.Value : 0;
                return intCheckResult;
            }
        }
        //// returns -1:downvoted, 1:upvoted, 0:not voted
        //public static SubmissionVoteTracker GetVote(voatEntities db, string userToCheck, int submissionID)
        //{
        //        var checkResult = db.SubmissionVoteTrackers.Where(u => u.UserName == userToCheck && u.SubmissionID == submissionID)
        //                .AsNoTracking()
        //                .FirstOrDefault();
        //}
        // a user has either upvoted or downvoted this submission earlier and wishes to reset the vote, delete the record
        public static void ResetMessageVote(string userWhichVoted, int submissionID)
        {
            using (var db = new voatEntities())
            {
                var votingTracker = db.SubmissionVoteTrackers.FirstOrDefault(b => b.SubmissionID == submissionID && b.UserName == userWhichVoted);

                if (votingTracker == null) return;
                //delete vote history
                db.SubmissionVoteTrackers.Remove(votingTracker);
                db.SaveChanges();
            }
        }
        // submit submission upvote
        public static void UpvoteSubmission(int submissionID, string userName, string clientIp)
        {
            // if you unlock this the bug is present

            object lockthis = _lockStore.GetLockObject(submissionID.ToString());
            lock (lockthis)
            {
                using (var db = new voatEntities())
                {
                    SubmissionVoteTracker previousVote = db.SubmissionVoteTrackers.Where(u => u.UserName == userName && u.SubmissionID == submissionID).FirstOrDefault();

                    Submission submission = db.Submissions.Find(submissionID);

                    if (submission.IsAnonymized)
                    {
                        // do not execute voting, subverse is in anonymized mode
                        return;
                    }

                    int result = (previousVote == null ? 0 : previousVote.VoteStatus.Value);

                    switch (result)
                    {
                        // never voted before
                        case 0:

                            if (submission.UserName != userName)
                            {
                                // check if this IP already voted on the same submission, abort voting if true
                                var ipVotedAlready = db.SubmissionVoteTrackers.Where(x => x.SubmissionID == submissionID && x.IPAddress == clientIp);
                                if (ipVotedAlready.Any())
                                    return;

                                submission.UpCount++;

                                //calculate new ranks
                                Ranking.RerankSubmission(submission);

                                // register upvote
                                var tmpVotingTracker = new SubmissionVoteTracker
                                {
                                    SubmissionID = submissionID,
                                    UserName = userName,
                                    VoteStatus = 1,
                                    CreationDate = Repository.CurrentDate,
                                    IPAddress = clientIp
                                };

                                db.SubmissionVoteTrackers.Add(tmpVotingTracker);
                                db.SaveChanges();
                                EventNotification.Instance.SendVoteNotice(submission.UserName, userName, Domain.Models.ContentType.Submission, submission.ID, 1);
                            }

                            break;

                        // downvoted before, turn downvote to upvote
                        case -1:

                            if (submission.UserName != userName)
                            {
                                submission.UpCount++;
                                submission.DownCount--;

                                //calculate new ranks
                                Ranking.RerankSubmission(submission);

                                previousVote.VoteStatus = 1;
                                previousVote.CreationDate = Repository.CurrentDate;

                                db.SaveChanges();
                                EventNotification.Instance.SendVoteNotice(submission.UserName, userName, Domain.Models.ContentType.Submission, submission.ID, 2);
                            }

                            break;

                        // upvoted before, reset
                        case 1:
                            {
                                submission.UpCount--;

                                //calculate new ranks
                                Ranking.RerankSubmission(submission);

                                db.SubmissionVoteTrackers.Remove(previousVote);
                                db.SaveChanges();
                                EventNotification.Instance.SendVoteNotice(submission.UserName, userName, Domain.Models.ContentType.Submission, submission.ID, -1);
                            }

                            break;
                    }
                }
            }
        }

        // submit submission downvote
        public static void DownvoteSubmission(int submissionID, string userName, string clientIp)
        {
            object lockthis = _lockStore.GetLockObject(submissionID.ToString());
            lock (lockthis)
            {
                using (var db = new voatEntities())
                {
                    Submission submission = db.Submissions.Find(submissionID);

                    SubmissionVoteTracker previousVote = db.SubmissionVoteTrackers.Where(u => u.UserName == userName && u.SubmissionID == submissionID).FirstOrDefault();

                    // do not execute downvoting if subverse is in anonymized mode
                    if (submission.IsAnonymized)
                    {
                        return;
                    }

                    // do not execute downvoting if submission is older than 7 days
                    var submissionPostingDate = submission.CreationDate;
                    TimeSpan timeElapsed = Repository.CurrentDate - submissionPostingDate;
                    if (timeElapsed.TotalDays > 7)
                    {
                        return;
                    }

                    // do not execute downvoting if user has insufficient CCP for target subverse
                    if (Karma.CommentKarmaForSubverse(userName, submission.Subverse) < submission.Subverse1.MinCCPForDownvote)
                    {
                        return;
                    }

                    int result = (previousVote == null ? 0 : previousVote.VoteStatus.Value);

                    switch (result)
                    {
                        // never voted before
                        case 0:
                            {
                                // this user is downvoting more than upvoting, don't register the downvote
                                if (UserHelper.IsUserCommentVotingMeanie(userName))
                                {
                                    return;
                                }

                                // check if this IP already voted on the same submission, abort voting if true
                                var ipVotedAlready = db.SubmissionVoteTrackers.Where(x => x.SubmissionID == submissionID && x.IPAddress == clientIp);
                                if (ipVotedAlready.Any())
                                    return;

                                submission.DownCount++;

                                //calculate new ranks
                                Ranking.RerankSubmission(submission);

                                // register downvote
                                var tmpVotingTracker = new SubmissionVoteTracker
                                {
                                    SubmissionID = submissionID,
                                    UserName = userName,
                                    VoteStatus = -1,
                                    CreationDate = Repository.CurrentDate,
                                    IPAddress = clientIp
                                };
                                db.SubmissionVoteTrackers.Add(tmpVotingTracker);
                                db.SaveChanges();

                                //SendVoteNotification(submission.UserName, "downvote");
                                EventNotification.Instance.SendVoteNotice(submission.UserName, userName, Domain.Models.ContentType.Submission, submission.ID, -1);
                            }

                            break;

                        // upvoted before, turn upvote to downvote
                        case 1:
                            {
                                submission.UpCount--;
                                submission.DownCount++;

                                //calculate new ranks
                                Ranking.RerankSubmission(submission);

                                // register Turn DownVote To UpVote
                                var votingTracker = db.SubmissionVoteTrackers.FirstOrDefault(b => b.SubmissionID == submissionID && b.UserName == userName);

                                previousVote.VoteStatus = -1;
                                previousVote.CreationDate = Repository.CurrentDate;

                                db.SaveChanges();

                                //SendVoteNotification(submission.UserName, "uptodownvote");
                                EventNotification.Instance.SendVoteNotice(submission.UserName, userName, Domain.Models.ContentType.Submission, submission.ID, -2);
                            }
                            break;

                        // downvoted before, reset
                        case -1:
                            {
                                //ResetMessageVote(userName, submissionID);
                                submission.DownCount--;

                                //calculate new ranks
                                Ranking.RerankSubmission(submission);

                                db.SubmissionVoteTrackers.Remove(previousVote);
                                db.SaveChanges();

                                //SendVoteNotification(submission.UserName, "upvote");
                                EventNotification.Instance.SendVoteNotice(submission.UserName, userName, Domain.Models.ContentType.Submission, submission.ID, 1);
                            }

                            break;
                    }
                }
            }
        }

    }
}