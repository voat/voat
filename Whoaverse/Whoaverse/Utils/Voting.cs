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

using System.Linq;
using Whoaverse.Models;

namespace Whoaverse.Utils
{
    public class Voting
    {
        //returns -1:downvoted, 1:upvoted, or 0:not voted
        public static int CheckIfVoted(string userToCheck, int messageId)
        {
            int intCheckResult = 0;

            using (whoaverseEntities db = new whoaverseEntities())
            {
                var checkResult = db.Votingtrackers
                                .Where(b => b.MessageId == messageId && b.UserName == userToCheck)
                                .FirstOrDefault();

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

        //returns -1:downvoted, 1:upvoted, or 0:not voted
        public static int CheckIfVotedComment(string userToCheck, int commentId)
        {
            int intCheckResult = 0;

            using (whoaverseEntities db = new whoaverseEntities())
            {
                var checkResult = db.Commentvotingtrackers
                                .Where(b => b.CommentId == commentId && b.UserName == userToCheck)
                                .FirstOrDefault();

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

        //a user has either upvoted or downvoted this submission earlier and wishes to reset the vote
        public static void ResetMessageVote(string userWhichVoted, int messageId)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var votingTracker = db.Votingtrackers
                                .Where(b => b.MessageId == messageId && b.UserName == userWhichVoted)
                                .FirstOrDefault();

                if (votingTracker != null)
                {
                    votingTracker.VoteStatus = 0;
                }
            }
        }

        //submit submission upvote
        public static async void UpvoteSubmission(int submissionId, string userWhichUpvoted)
        {
            int result = Voting.CheckIfVoted(userWhichUpvoted, submissionId);

            using (whoaverseEntities db = new whoaverseEntities())
            {
                Message submission = db.Messages.Find(submissionId);

                switch (result)
                {
                    //never voted before
                    case 0:

                        if (submission != null)
                        {
                            double submissionAge = Whoaverse.Utils.Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge);

                            submission.Likes++;
                            submission.Rank = newRank;

                            //register upvote
                            Votingtracker tmpVotingTracker = new Votingtracker();
                            tmpVotingTracker.MessageId = submissionId;
                            tmpVotingTracker.UserName = userWhichUpvoted;
                            tmpVotingTracker.VoteStatus = 1;
                            db.Votingtrackers.Add(tmpVotingTracker);
                            await db.SaveChangesAsync();
                        }

                        break;

                    //downvoted before, turn downvote to upvote
                    case -1:

                        if (submission != null)
                        {
                            double submissionAge = Whoaverse.Utils.Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge);

                            submission.Likes++;
                            submission.Dislikes--;
                            submission.Rank = newRank;
                            
                            //register Turn DownVote To UpVote
                            var votingTracker = db.Votingtrackers
                                .Where(b => b.MessageId == submissionId && b.UserName == userWhichUpvoted)
                                .FirstOrDefault();
                            
                            if (votingTracker != null)
                            {
                                votingTracker.VoteStatus = 1;
                            }
                        }

                        break;

                    //upvoted before, reset
                    case 1:

                        if (submission != null)
                        {
                            double submissionAge = Whoaverse.Utils.Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge);

                            submission.Likes--;
                            submission.Rank = newRank;

                            ResetMessageVote(userWhichUpvoted, submissionId);
                        }

                        break;

                }
            }

        }

        //submit submission downvote
        public static async void DownvoteSubmission(int submissionId, string userWhichDownvoted)
        {
            int result = Voting.CheckIfVoted(userWhichDownvoted, submissionId);

            using (whoaverseEntities db = new whoaverseEntities())
            {
                Message submission = db.Messages.Find(submissionId);

                switch (result)
                {
                    //never voted before
                    case 0:

                        if (submission != null)
                        {
                            double submissionAge = Whoaverse.Utils.Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge);

                            submission.Dislikes++;
                            submission.Rank = newRank;

                            //register downvote
                            Votingtracker tmpVotingTracker = new Votingtracker();
                            tmpVotingTracker.MessageId = submissionId;
                            tmpVotingTracker.UserName = userWhichDownvoted;
                            tmpVotingTracker.VoteStatus = -1;
                            db.Votingtrackers.Add(tmpVotingTracker);
                            await db.SaveChangesAsync();
                        }

                        break;

                    //upvoted before, turn upvote to downvote
                    case -1:

                        if (submission != null)
                        {
                            double submissionAge = Whoaverse.Utils.Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge);

                            submission.Likes--;
                            submission.Dislikes++;
                            submission.Rank = newRank;

                            //register Turn DownVote To UpVote
                            var votingTracker = db.Votingtrackers
                                .Where(b => b.MessageId == submissionId && b.UserName == userWhichDownvoted)
                                .FirstOrDefault();

                            if (votingTracker != null)
                            {
                                votingTracker.VoteStatus = -1;
                            }
                        }

                        break;

                    //downvoted before, reset
                    case 1:

                        if (submission != null)
                        {
                            double submissionAge = Whoaverse.Utils.Submissions.CalcSubmissionAgeDouble(submission.Date);
                            double newRank = Ranking.CalculateNewRank(submission.Rank, submissionAge);

                            submission.Dislikes--;
                            submission.Rank = newRank;

                            ResetMessageVote(userWhichDownvoted, submissionId);
                        }

                        break;

                }
            }

        }

    }
}