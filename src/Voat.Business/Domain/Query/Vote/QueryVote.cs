using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryVote : CachedQuery<Domain.Models.Vote>
    {
        private int _id;
        public QueryVote(int id) : base(new Caching.CachePolicy(TimeSpan.FromMinutes(60)))
        {
            _id = id;
        }
        protected override string FullCacheKey => Caching.CachingKey.Vote(_id);
        public override string CacheKey => Caching.CachingKey.Vote(_id);

        protected override async Task<Vote> GetData()
        {
            using (var repo = new Repository(User))
            {
                return await repo.GetVote(_id);
            }
        }
    }
}
