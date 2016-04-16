using Voat.Common;

namespace Voat.Domain.Models
{
    public interface IVoteStatus
    {
        int? Vote { get; }
    }

    public abstract class VoteableObject : Score, IVoteStatus
    {
        /// <summary>
        /// The vote status of domain type. Null (user not logged in), 0 (unvoted), -1 (downvoted), 1 (upvoted)
        /// </summary>
        public int? Vote { get; set; }
    }
}
