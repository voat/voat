using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryFilters : CachedQuery<IEnumerable<Filter>>
    {
        
        public QueryFilters() : base(new CachePolicy(TimeSpan.FromDays(7)))
        {
        }

        public override string CacheKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.Filters();
            }
        }

        protected override async Task<IEnumerable<Filter>> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetFilters(true);
            }
        }
    }
}
