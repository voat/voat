namespace Voat.Queries.Voting
{
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class CommentVote
    {
        public static Task<VoteStatus> CheckCommentForVoteAsync(this IQueryable<Commentvotingtracker> query, string userName,
            int commentId)
        {
            return query.Where(v => v.CommentId == commentId).CheckForVoteAsync(userName);
        }
    }
}