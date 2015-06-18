namespace Voat.Queries.Voting
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public enum VoteStatus
    {
        None,
        Downvoted,
        Upvoted
    }

    public static class CommentVote
    {
        public static async Task<VoteStatus> CheckCommentForVoteAsync(this DbContext context, string userName,
            int commentId)
        {
            var result =
                await
                    context.Set<Commentvotingtracker>()
                        .Where(v => v.CommentId == commentId && v.UserName == userName)
                        .Select(v => v.VoteStatus)
                        .DefaultIfEmpty(0)
                        .FirstAsync()
                        .ConfigureAwait(false);

            if (result == 0)
            {
                return VoteStatus.None;
            }

            return result == 1 ? VoteStatus.Upvoted : VoteStatus.Downvoted;
        }
    }
}