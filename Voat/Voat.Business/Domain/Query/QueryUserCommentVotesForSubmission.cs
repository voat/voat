using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryUserCommentVotesForSubmission : CachedQuery<IEnumerable<Data.Models.CommentVoteTracker>>
    {
        protected int _submissionID;

        public QueryUserCommentVotesForSubmission(int submissionID) : this(submissionID, new CachePolicy(TimeSpan.FromMinutes(5)))
        {
        }

        public QueryUserCommentVotesForSubmission(int submissionID, CachePolicy policy) : base(policy)
        {
            this._submissionID = submissionID;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}:{1}", UserName, _submissionID);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserCommentVotes(UserName, _submissionID);
            }
        }

        protected override async Task<IEnumerable<Data.Models.CommentVoteTracker>> GetData()
        {
            using (var repo = new Repository())
            {
                var result = repo.UserCommentVotesBySubmission(_submissionID, UserName);
                return (result == null || !result.Any() ? null : result);
            }
        }
    }
   
}
