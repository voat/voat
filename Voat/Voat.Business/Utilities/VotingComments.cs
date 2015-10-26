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
using System.Linq;
//using Microsoft.AspNet.SignalR;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public class VotingComments
    {
        // submit comment upvote
        public static void UpvoteComment(int commentId, string userWhichUpvoted, string clientIpHash)
        {
            int result = CheckIfVotedComment(userWhichUpvoted, commentId);

            using (voatEntities db = new voatEntities())
            {
                Comment comment = db.Comments.Find(commentId);

                if (comment.Submission.IsAnonymized)
                {
                    // do not execute voting, subverse is in anonymized mode
                    return;
                }                

                switch (result)
                {
                    // never voted before
                    case 0:

                        if (comment.UserName != userWhichUpvoted)
                        {
                            // check if this IP already voted on the same comment, abort voting if true
                            var ipVotedAlready = db.CommentVoteTrackers.Where(x => x.CommentID == commentId && x.IPAddress == clientIpHash);
                            if (ipVotedAlready.Any()) return;

                            comment.UpCount++;

                            // register upvote
                            var tmpVotingTracker = new CommentVoteTracker
                            {
                                CommentID = commentId,
                                UserName = userWhichUpvoted,
                                VoteStatus = 1,
                                CreationDate = DateTime.Now,
                                IPAddress = clientIpHash
                            };
                            db.CommentVoteTrackers.Add(tmpVotingTracker);
                            db.SaveChanges();

                            Voting.SendVoteNotification(comment.UserName, "upvote");
                        }

                        break;

                    // downvoted before, turn downvote to upvote
                    case -1:

                        if (comment.UserName != userWhichUpvoted)
                        {
                            comment.UpCount++;
                            comment.DownCount--;

                            // register Turn DownVote To UpVote
                            var votingTracker = db.CommentVoteTrackers.FirstOrDefault(b => b.CommentID == commentId && b.UserName == userWhichUpvoted);

                            if (votingTracker != null)
                            {
                                votingTracker.VoteStatus = 1;
                                votingTracker.CreationDate = DateTime.Now;
                            }
                            db.SaveChanges();

                            Voting.SendVoteNotification(comment.UserName, "downtoupvote");
                        }

                        break;

                    // upvoted before, reset
                    case 1:

                        comment.UpCount--;
                        db.SaveChanges();

                        Voting.SendVoteNotification(comment.UserName, "downvote");

                        ResetCommentVote(userWhichUpvoted, commentId);

                        break;
                }
            }

        }

        // submit submission downvote
        public static void DownvoteComment(int commentId, string userWhichDownvoted, string clientIpHash)
        {
            int result = CheckIfVotedComment(userWhichDownvoted, commentId);

            using (voatEntities db = new voatEntities())
            {
                Comment comment = db.Comments.Find(commentId);

                // do not execute downvoting, subverse is in anonymized mode
                if (comment.Submission.IsAnonymized)
                {
                    return;
                }

                // do not execute downvoting if user has insufficient CCP for target subverse
                if (Karma.CommentKarmaForSubverse(userWhichDownvoted, comment.Submission.Subverse) < comment.Submission.Subverse1.MinCCPForDownvote)
                {
                    return;
                }

                // do not execute downvoting if comment is older than 7 days
                var commentPostingDate = comment.CreationDate;
                TimeSpan timeElapsed = DateTime.Now - commentPostingDate;
                if (timeElapsed.TotalDays > 7)
                {
                    return;
                }

                switch (result)
                {
                    // never voted before
                    case 0:

                    {
                        // this user is downvoting more than upvoting, don't register the downvote
                        if (UserHelper.IsUserCommentVotingMeanie(userWhichDownvoted))
                        {
                            return;
                        }

                        // check if this IP already voted on the same comment, abort voting if true
                        var ipVotedAlready = db.CommentVoteTrackers.Where(x => x.CommentID == commentId && x.IPAddress == clientIpHash);
                        if (ipVotedAlready.Any()) return;

                        comment.DownCount++;

                        // register downvote
                        var tmpVotingTracker = new CommentVoteTracker
                        {
                            CommentID = commentId,
                            UserName = userWhichDownvoted,
                            VoteStatus = -1,
                            CreationDate = DateTime.Now,
                            IPAddress = clientIpHash
                        };
                        db.CommentVoteTrackers.Add(tmpVotingTracker);
                        db.SaveChanges();

                            Voting.SendVoteNotification(comment.UserName, "downvote");
                    }

                        break;

                    // upvoted before, turn upvote to downvote
                    case 1:

                    {
                        comment.UpCount--;
                        comment.DownCount++;                            

                        //register Turn DownVote To UpVote
                        var votingTracker = db.CommentVoteTrackers.FirstOrDefault(b => b.CommentID == commentId && b.UserName == userWhichDownvoted);

                        if (votingTracker != null)
                        {
                            votingTracker.VoteStatus = -1;
                            votingTracker.CreationDate = DateTime.Now;
                        }
                        db.SaveChanges();

                            Voting.SendVoteNotification(comment.UserName, "uptodownvote");
                    }

                        break;

                    // downvoted before, reset
                    case -1:

                        comment.DownCount--;
                        db.SaveChanges();
                        ResetCommentVote(userWhichDownvoted, commentId);

                        Voting.SendVoteNotification(comment.UserName, "upvote");

                        break;
                }
            }

        }

        // returns -1:downvoted, 1:upvoted, or 0:not voted
        public static int CheckIfVotedComment(string userToCheck, int commentId)
        {
            int intCheckResult = 0;

            using (var db = new voatEntities())
            {
                var checkResult = db.CommentVoteTrackers.FirstOrDefault(b => b.CommentID == commentId && b.UserName == userToCheck);

                intCheckResult = checkResult != null ? checkResult.VoteStatus.Value : 0;

                return intCheckResult;
            }

        }

        // a user has either upvoted or downvoted this submission earlier and wishes to reset the vote, delete the record
        public static void ResetCommentVote(string userWhichVoted, int commentId)
        {
            using (var db = new voatEntities())
            {
                var votingTracker = db.CommentVoteTrackers.FirstOrDefault(b => b.CommentID == commentId && b.UserName == userWhichVoted);

                if (votingTracker == null) return;
                // delete vote history
                db.CommentVoteTrackers.Remove(votingTracker);
                db.SaveChanges();
            }
        }

        //This code is repeated in Voating.cs
        //// send SignalR realtime notification of incoming commentvote to the author
        //private static void SendVoteNotification(string userName, string notificationType)
        //{
        //    ////MIGRATION HACK
        //    //var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();

        //    //switch (notificationType)
        //    //{
        //    //    case "downvote":
        //    //        {
        //    //            hubContext.Clients.User(userName).incomingDownvote(2);
        //    //        }
        //    //        break;
        //    //    case "upvote":
        //    //        {
        //    //            hubContext.Clients.User(userName).incomingUpvote(2);
        //    //        }
        //    //        break;
        //    //    case "downtoupvote":
        //    //        {
        //    //            hubContext.Clients.User(userName).incomingDownToUpvote(2);
        //    //        }
        //    //        break;
        //    //    case "uptodownvote":
        //    //        {
        //    //            hubContext.Clients.User(userName).incomingUpToDownvote(2);
        //    //        }
        //    //        break;
        //    //}
        //}
    }
}