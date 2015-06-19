namespace Voat.Commands.Voting
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Models;
    using Queries.Voting;

    public static class CommentVote
    {
        public static async Task<VoteDirection> UpvoteCommentAsync(this DbContext context, int commentId, string userName, string clientIpHash)
        {
            var comment = await context.Set<Comment>().FindAsync(commentId).ConfigureAwait(false);

            if (comment.Message.Anonymized)
            {
                return VoteDirection.None;
            }

            var currentVoteStatus = await context.CheckCommentForVoteAsync(userName, commentId).ConfigureAwait(false);

            switch (currentVoteStatus)
            {
                case VoteStatus.None:
                    if (comment.Name == userName)
                    {
                        return VoteDirection.None;
                    }

                    var votedAlready =
                        await context.Set<Commentvotingtracker>()
                            .AnyAsync(c => c.CommentId == commentId && c.ClientIpAddress == clientIpHash)
                            .ConfigureAwait(false);

                    if (votedAlready)
                    {
                        return VoteDirection.None;
                    }

                    comment.Likes++;
                    context.Set<Commentvotingtracker>().Add(new Commentvotingtracker
                    {
                        CommentId = commentId,
                        UserName = userName,
                        VoteStatus = 1,
                        Timestamp = DateTime.Now,
                        ClientIpAddress = clientIpHash
                    });

                    await context.SaveChangesAsync().ConfigureAwait(false);
                    return VoteDirection.Upvote;

                case VoteStatus.Downvoted:
                    if (comment.Name == userName)
                    {
                        return VoteDirection.None;
                    }

                    comment.Likes++;
                    comment.Dislikes--;

                    var trackerEntry =
                        await context.Set<Commentvotingtracker>()
                            .FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserName == userName)
                            .ConfigureAwait(false);

                    // TODO: Clarification - what should happen if the tracker entry is missing? Shouldn't tracked entities have append-only behavior?
                    if (trackerEntry != null)
                    {
                        trackerEntry.VoteStatus = 1;
                        trackerEntry.Timestamp = DateTime.Now;
                    }

                    await context.SaveChangesAsync().ConfigureAwait(false);

                    return VoteDirection.DownvoteToUpvote;

                case VoteStatus.Upvoted:
                    comment.Likes--;

                    var entryToRemove =
                        await
                            context.Set<Commentvotingtracker>()
                                .FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserName == userName)
                                .ConfigureAwait(false);

                    // TODO: As above, shouldn't tracked entities have append-only behavior?
                    if (entryToRemove != null)
                    {
                        context.Set<Commentvotingtracker>().Remove(entryToRemove);
                    }
                    await context.SaveChangesAsync().ConfigureAwait(false);
                    return VoteDirection.Downvote;

                default:
                    return VoteDirection.None;
            }
        }
    }
}