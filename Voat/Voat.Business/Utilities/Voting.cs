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
//using Microsoft.AspNet.SignalR;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class Voting
    {
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
            //// user account voting check
            //int result = CheckIfVoted(userName, submissionID);

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
                            if (ipVotedAlready.Any()) return;

                            submission.UpCount++;

                            //calculate new ranks
                            Ranking.RerankSubmission(submission);

                            // register upvote
                            var tmpVotingTracker = new SubmissionVoteTracker
                            {
                                SubmissionID = submissionID,
                                UserName = userName,
                                VoteStatus = 1,
                                CreationDate = DateTime.Now,
                                IPAddress = clientIp
                            };

                            db.SubmissionVoteTrackers.Add(tmpVotingTracker);
                            db.SaveChanges();

                            SendVoteNotification(submission.UserName, "upvote");
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
                            previousVote.CreationDate = DateTime.Now;

                            db.SaveChanges();

                            SendVoteNotification(submission.UserName, "downtoupvote");
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

                            //ResetMessageVote(userName, submissionID);

                            SendVoteNotification(submission.UserName, "downvote");
                        }

                        break;
                }
            }

        }

        // submit submission downvote
        public static void DownvoteSubmission(int submissionID, string userName, string clientIp)
        {
            //int result = CheckIfVoted(userWhichDownvoted, submissionId);

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
                TimeSpan timeElapsed = DateTime.Now - submissionPostingDate;
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
                            if (ipVotedAlready.Any()) return;

                            submission.DownCount++;

                            //calculate new ranks
                            Ranking.RerankSubmission(submission);

                            // register downvote
                            var tmpVotingTracker = new SubmissionVoteTracker
                            {
                                SubmissionID = submissionID,
                                UserName = userName,
                                VoteStatus = -1,
                                CreationDate = DateTime.Now,
                                IPAddress = clientIp
                            };
                            db.SubmissionVoteTrackers.Add(tmpVotingTracker);
                            db.SaveChanges();

                            SendVoteNotification(submission.UserName, "downvote");
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
                            previousVote.CreationDate = DateTime.Now;

                            db.SaveChanges();

                            SendVoteNotification(submission.UserName, "uptodownvote");
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

                            SendVoteNotification(submission.UserName, "upvote");
                        }

                        break;
                }
            }
        }

        // send SignalR realtime notification of incoming vote to the author
        public static void SendVoteNotification(string userName, string notificationType)
        {
            //MIGRATION HACK: 
            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();

            //switch (notificationType)
            //{
            //    case "downvote":
            //        {
            //            hubContext.Clients.User(userName).incomingDownvote(1);
            //        }
            //        break;
            //    case "upvote":
            //        {
            //            hubContext.Clients.User(userName).incomingUpvote(1);
            //        }
            //        break;
            //    case "downtoupvote":
            //        {
            //            hubContext.Clients.User(userName).incomingDownToUpvote(1);
            //        }
            //        break;
            //    case "uptodownvote":
            //        {
            //            hubContext.Clients.User(userName).incomingUpToDownvote(1);
            //        }
            //        break;
            //    case "commentdownvote":
            //        {
            //            hubContext.Clients.User(userName).incomingDownvote(2);
            //        }
            //        break;
            //    case "commentupvote":
            //        {
            //            hubContext.Clients.User(userName).incomingUpvote(2);
            //        }
            //        break;
            //    case "commentdowntoupvote":
            //        {
            //            hubContext.Clients.User(userName).incomingDownToUpvote(2);
            //        }
            //        break;
            //    case "commentuptodownvote":
            //        {
            //            hubContext.Clients.User(userName).incomingUpToDownvote(2);
            //        }
            //        break;
            //}
        }
                           
    }
}