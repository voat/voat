using System;
using System.Collections.Generic;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryApiCorsPolicies : CachedQuery<IEnumerable<ApiCorsPolicy>>
    {
        public QueryApiCorsPolicies() : base(new CachePolicy(TimeSpan.FromMinutes(10)))
        {
        }

        public override string CacheKey
        {
            get
            {
                return "Cors";
            }
        }

        protected override string FullCacheKey
        {
            get { return CachingKey.ApiCorsPolicies(); }
        }

        protected override IEnumerable<ApiCorsPolicy> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetApiCorsPolicies();
            }
        }
    }
}
