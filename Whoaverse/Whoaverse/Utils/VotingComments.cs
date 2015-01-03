/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Linq;
using Voat.Models;

namespace Voat.Utils
{
    public class VotingComments
    {
        // submit comment upvote
        public static void UpvoteComment(int commentId, string userWhichUpvoted)
        {
            int result = CheckIfVotedComment(userWhichUpvoted, commentId);

            using (whoaverseEntities db = new whoaverseEntities())
            {
                Comment comment = db.Comments.Find(commentId);

                if (comment.Message.Anonymized)
                {
                    // do not execute voting, subverse is in anonymized mode
                    return;
                }                

                switch (result)
                {
                    // never voted before
                    case 0:

                        if (comment.Name != userWhichUpvoted)
                        {
                            comment.Likes++;

                            // register upvote
                            var tmpVotingTracker = new Commentvotingtracker
                            {
                                CommentId = commentId,
                                UserName = userWhichUpvoted,
                                VoteStatus = 1,
                                Timestamp = DateTime.Now
                            };
                            db.Commentvotingtrackers.Add(tmpVotingTracker);
                            db.SaveChanges();
                        }

                        break;

                    // downvoted before, turn downvote to upvote
                    case -1:

                        if (comment.Name != userWhichUpvoted)
                        {
                            comment.Likes++;
                            comment.Dislikes--;

                            // register Turn DownVote To UpVote
                            var votingTracker = db.Commentvotingtrackers.FirstOrDefault(b => b.CommentId == commentId && b.UserName == userWhichUpvoted);

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

                        comment.Likes--;
                        db.SaveChanges();

                        ResetCommentVote(userWhichUpvoted, commentId);

                        break;
                }
            }

        }

        // submit submission downvote
        public static void DownvoteComment(int commentId, string userWhichDownvoted)
        {
            int result = CheckIfVotedComment(userWhichDownvoted, commentId);

            using (whoaverseEntities db = new whoaverseEntities())
            {
                Comment comment = db.Comments.Find(commentId);

                if (comment.Message.Anonymized)
                {
                    // do not execute voting, subverse is in anonymized mode
                    return;
                }  

                switch (result)
                {
                    // never voted before
                    case 0:

                    {
                        comment.Dislikes++;

                        // register downvote
                        var tmpVotingTracker = new Commentvotingtracker
                        {
                            CommentId = commentId,
                            UserName = userWhichDownvoted,
                            VoteStatus = -1,
                            Timestamp = DateTime.Now
                        };
                        db.Commentvotingtrackers.Add(tmpVotingTracker);
                        db.SaveChanges();
                    }

                        break;

                    // upvoted before, turn upvote to downvote
                    case 1:

                    {
                        comment.Likes--;
                        comment.Dislikes++;                            

                        //register Turn DownVote To UpVote
                        var votingTracker = db.Commentvotingtrackers.FirstOrDefault(b => b.CommentId == commentId && b.UserName == userWhichDownvoted);

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

                        comment.Dislikes--;
                        db.SaveChanges();
                        ResetCommentVote(userWhichDownvoted, commentId);

                        break;
                }
            }

        }

        // returns -1:downvoted, 1:upvoted, or 0:not voted
        public static int CheckIfVotedComment(string userToCheck, int commentId)
        {
            int intCheckResult = 0;

            using (var db = new whoaverseEntities())
            {
                var checkResult = db.Commentvotingtrackers.FirstOrDefault(b => b.CommentId == commentId && b.UserName == userToCheck);

                intCheckResult = checkResult != null ? checkResult.VoteStatus.Value : 0;

                return intCheckResult;
            }

        }

        // a user has either upvoted or downvoted this submission earlier and wishes to reset the vote, delete the record
        public static void ResetCommentVote(string userWhichVoted, int commentId)
        {
            using (var db = new whoaverseEntities())
            {
                var votingTracker = db.Commentvotingtrackers.FirstOrDefault(b => b.CommentId == commentId && b.UserName == userWhichVoted);

                if (votingTracker == null) return;
                // delete vote history
                db.Commentvotingtrackers.Remove(votingTracker);
                db.SaveChanges();
            }
        }
    }
}