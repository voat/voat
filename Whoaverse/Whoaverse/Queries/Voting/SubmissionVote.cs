namespace Voat.Queries.Voting
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class SubmissionVote
    {
        public static Task<VoteStatus> CheckSubmissionForVoteAsync(this DbContext context, string userName,
            int messageId)
        {
            return context.Set<Votingtracker>().Where(v => v.MessageId == messageId).CheckForVoteAsync(userName);
        }
    }
}