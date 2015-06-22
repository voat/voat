namespace Voat.Queries.Voting
{
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class SubmissionVote
    {
        public static Task<VoteStatus> CheckSubmissionForVoteAsync(this IQueryable<Votingtracker> query, string userName,
            int messageId)
        {
            return query.Where(v => v.MessageId == messageId).CheckForVoteAsync(userName);
        }
    }
}