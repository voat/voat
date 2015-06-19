namespace Voat.Queries.Voting
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    internal static class Vote
    {
        public static async Task<VoteStatus> CheckForVoteAsync(this IQueryable<IVoteTracked> query, string userName)
        {
            var result = await query.Where(q => q.UserName == userName)
                .Select(q => q.VoteStatus)
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