using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whoaverse.Models;

namespace Whoaverse.Utils
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

                        if (comment != null && comment.Name != userWhichUpvoted)
                        {
                            comment.Likes++;

                            // register upvote
                            Commentvotingtracker tmpVotingTracker = new Commentvotingtracker();
                            tmpVotingTracker.CommentId = commentId;
                            tmpVotingTracker.UserName = userWhichUpvoted;
                            tmpVotingTracker.VoteStatus = 1;
                            tmpVotingTracker.Timestamp = DateTime.Now;
                            db.Commentvotingtrackers.Add(tmpVotingTracker);
                            db.SaveChanges();
                        }

                        break;

                    // downvoted before, turn downvote to upvote
                    case -1:

                        if (comment != null && comment.Name != userWhichUpvoted)
                        {
                            comment.Likes++;
                            comment.Dislikes--;

                            // register Turn DownVote To UpVote
                            var votingTracker = db.Commentvotingtrackers
                                .Where(b => b.CommentId == commentId && b.UserName == userWhichUpvoted)
                                .FirstOrDefault();

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

                        if (comment != null)
                        {
                            comment.Likes--;
                            db.SaveChanges();

                            ResetCommentVote(userWhichUpvoted, commentId);
                        }

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

                        if (comment != null)
                        {
                            comment.Dislikes++;

                            // register downvote
                            Commentvotingtracker tmpVotingTracker = new Commentvotingtracker();
                            tmpVotingTracker.CommentId = commentId;
                            tmpVotingTracker.UserName = userWhichDownvoted;
                            tmpVotingTracker.VoteStatus = -1;
                            tmpVotingTracker.Timestamp = DateTime.Now;
                            db.Commentvotingtrackers.Add(tmpVotingTracker);
                            db.SaveChanges();
                        }

                        break;

                    // upvoted before, turn upvote to downvote
                    case 1:

                        if (comment != null)
                        {
                            comment.Likes--;
                            comment.Dislikes++;                            

                            //register Turn DownVote To UpVote
                            var votingTracker = db.Commentvotingtrackers
                                .Where(b => b.CommentId == commentId && b.UserName == userWhichDownvoted)
                                .FirstOrDefault();

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

                        if (comment != null)
                        {
                            comment.Dislikes--;
                            db.SaveChanges();
                            ResetCommentVote(userWhichDownvoted, commentId);
                        }

                        break;
                }
            }

        }

        // returns -1:downvoted, 1:upvoted, or 0:not voted
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

        // a user has either upvoted or downvoted this submission earlier and wishes to reset the vote, delete the record
        public static void ResetCommentVote(string userWhichVoted, int commentId)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var votingTracker = db.Commentvotingtrackers
                                .Where(b => b.CommentId == commentId && b.UserName == userWhichVoted)
                                .FirstOrDefault();

                if (votingTracker != null)
                {
                    // delete vote history
                    db.Commentvotingtrackers.Remove(votingTracker);
                    db.SaveChanges();
                }
            }
        }
    }
}