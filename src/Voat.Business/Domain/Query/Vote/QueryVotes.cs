using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryVotes : CachedQuery<IEnumerable<Domain.Models.Vote>>
    {
        private string _subverse;
        private SearchOptions _options;

        protected override string FullCacheKey => CachingKey.Votes(_subverse, _options);

        public override string CacheKey => throw new NotImplementedException();


        public QueryVotes(string subverse, SearchOptions options) : base(new Caching.CachePolicy(TimeSpan.FromMinutes(60)))
        {
            _subverse = subverse;
            _options = options;
        }

        protected override async Task<IEnumerable<Vote>> GetData()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.GetVotes(_subverse, _options);
                
                //Add to cache
                if (result != null && result.Any())
                {
                    result.ForEach(x => CacheHandler.Replace(CachingKey.Vote(x.ID), x, TimeSpan.FromMinutes(30)));
                }

                return result;
            }
        }
    }
}
