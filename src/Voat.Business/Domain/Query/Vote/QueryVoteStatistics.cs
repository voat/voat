using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Voting.Models;

namespace Voat.Domain.Query
{
    public class QueryVoteStatistics : CachedQuery<VoteStatistics>
    {
        private int _id;
        public QueryVoteStatistics(int id) : base(new Caching.CachePolicy(TimeSpan.FromMinutes(20)))
        {
            _id = id;
        }

        protected override string FullCacheKey => Caching.CachingKey.VoteStatistics(_id);
        public override string CacheKey => Caching.CachingKey.VoteStatistics(_id);

        protected override async Task<VoteStatistics> GetData()
        {
            using (var repo = new Repository(User))
            {
                return await repo.GetVoteStatistics(_id);
            }
        }
    }
}
