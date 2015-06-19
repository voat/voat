namespace Voat.Queries.Voting
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class CommentVote
    {
        public static Task<VoteStatus> CheckCommentForVoteAsync(this DbContext context, string userName,
            int commentId)
        {
            return context.Set<Commentvotingtracker>().Where(v => v.CommentId == commentId).CheckForVoteAsync(userName);
        }
    }
}