using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryBannedDomains : CachedQuery<IEnumerable<BannedDomain>>
    {
        public QueryBannedDomains() : this(new CachePolicy(TimeSpan.FromHours(1)))
        {
        }

        public QueryBannedDomains(CachePolicy policy) : base(policy)
        {
        }

        public override string CacheKey
        {
            get
            {
                return "Hosts";
            }
        }

        protected override async Task<IEnumerable<BannedDomain>> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetBannedDomains();
            }
        }
    }
}
