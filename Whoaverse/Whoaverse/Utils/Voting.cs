/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using System.Linq;
using Whoaverse.Models;

namespace Whoaverse.Utils
{
    public static class Voting
    {
        // returns -1:downvoted, 1:upvoted, 0:not voted
        public static int CheckIfVoted(string userToCheck, int messageId)
        {
            int intCheckResult = 0;

            using (var db = new whoaverseEntities())
            {
                var checkResult = db.Votingtrackers.FirstOrDefault(b => b.MessageId == messageId && b.UserName == userToCheck);

                if (checkResult != null)
                {
                    intCheckResult = checkResult.VoteStatus.Value;
                }
                else
                {
                    intCheckResult = 0;
                }

                return intCheckResult;
            }

        }

        // a user has either upvoted or downvoted this submission earlier and wishes to reset the vote, delete the record
        public static void ResetMessageVote(string userWhichVoted, int messageId)
        {
            using (var db = new whoaverseEntities())
            {
                var votingTracker = db.Votingtrackers.FirstOrDefault(b => b.MessageId == messageId && b.UserName == userWhichVoted);

                if (votingTracker == null) return;
                //delete vote history
                db.Votingtrackers.Remove(votingTracker);
                db.SaveChanges();
            }
        }

        // submit submission upvote
        public static void UpvoteSubmission(int submissionId, string userWhichUpvoted)
        {
            int result = CheckIfVoted(userWhichUpvoted, submissionId);

            using (var db = new whoaverseEntities())
            {
                Message submission = db.Messages.Find(submissionId);

                if (submission.Anonymized)
                {
                    // do not execute voting, subverse is in anonymized mode
                    return;
                }

                switch (result)
                {
                    //never voted before
                    case 0:

                        if (submission.Name != userWhichUpvoted)
                        {
                            submission.Likes++;
                            double currentScore = submission.Likes - submission.Dislikes;
                            double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);
                            submission.Rank = newRank;

                            //register upvote
                            var tmpVotingTracker = new Votingtracker
                            {
                                MessageId = submissionId,
                                UserName = userWhichUpvoted,
                                VoteStatus = 1,
                                Timestamp = DateTime.Now
                            };
                            db.Votingtrackers.Add(tmpVotingTracker);
                            db.SaveChanges();
                        }

                        break;

                    // downvoted before, turn downvote to upvote
                    case -1:

                        if (submission.Name != userWhichUpvoted)
                        {
                            submission.Likes++;
                            submission.Dislikes--;

                            double currentScore = submission.Likes - submission.Dislikes;
                            double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);
                            submission.Rank = newRank;

                            //register Turn DownVote To UpVote
                            var votingTracker = db.Votingtrackers.FirstOrDefault(b => b.MessageId == submissionId && b.UserName == userWhichUpvoted);

                            if (votingTracker != null)
                            {
                                votingTracker.VoteStatus = 1;
                                votingTracker.Timestamp = DateTime.Now;
                            }
                            db.SaveChanges();
                        }

                        break;

                    // upvoted before, reset
                    case 1:
                        {
                            submission.Likes--;

                            double currentScore = submission.Likes - submission.Dislikes;
                            double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);

                            submission.Rank = newRank;
                            db.SaveChanges();

                            ResetMessageVote(userWhichUpvoted, submissionId);
                        }

                        break;
                }
            }

        }

        // submit submission downvote
        public static void DownvoteSubmission(int submissionId, string userWhichDownvoted)
        {
            int result = CheckIfVoted(userWhichDownvoted, submissionId);

            using (var db = new whoaverseEntities())
            {
                Message submission = db.Messages.Find(submissionId);

                if (submission.Anonymized)
                {
                    // do not execute voting, subverse is in anonymized mode
                    return;
                }

                switch (result)
                {
                    // never voted before
                    case 0:
                        {
                            submission.Dislikes++;

                            double currentScore = submission.Likes - submission.Dislikes;
                            double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);

                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);

                            submission.Rank = newRank;

                            // register downvote
                            var tmpVotingTracker = new Votingtracker
                            {
                                MessageId = submissionId,
                                UserName = userWhichDownvoted,
                                VoteStatus = -1,
                                Timestamp = DateTime.Now
                            };
                            db.Votingtrackers.Add(tmpVotingTracker);
                            db.SaveChanges();
                        }

                        break;

                    // upvoted before, turn upvote to downvote
                    case 1:
                        {
                            submission.Likes--;
                            submission.Dislikes++;

                            double currentScore = submission.Likes - submission.Dislikes;
                            double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);

                            submission.Rank = newRank;

                            // register Turn DownVote To UpVote
                            var votingTracker = db.Votingtrackers.FirstOrDefault(b => b.MessageId == submissionId && b.UserName == userWhichDownvoted);

                            if (votingTracker != null)
                            {
                                votingTracker.VoteStatus = -1;
                                votingTracker.Timestamp = DateTime.Now;
                            }
                            db.SaveChanges();
                        }

                        break;

                    // downvoted before, reset
                    case -1:
                        {
                            submission.Dislikes--;

                            double currentScore = submission.Likes - submission.Dislikes;
                            double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge, currentScore);

                            submission.Rank = newRank;
                            db.SaveChanges();

                            ResetMessageVote(userWhichDownvoted, submissionId);
                        }

                        break;

                }
            }

        }

    }
}