using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryApiThrottlePolicy : CachedQuery<ApiThrottlePolicy>
    {
        private int _apiThrottlePolicyID = 0;

        public QueryApiThrottlePolicy(int apiThrottlePolicyID) : base(new CachePolicy(TimeSpan.FromMinutes(5)))
        {
            _apiThrottlePolicyID = apiThrottlePolicyID;
        }

        public override string CacheKey
        {
            get { return _apiThrottlePolicyID.ToString(); }
        }

        protected override string FullCacheKey
        {
            get { return CachingKey.ApiThrottlePolicy(_apiThrottlePolicyID); }
        }

        protected override async Task<ApiThrottlePolicy> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetApiThrottlePolicy(_apiThrottlePolicyID);
            }
        }
    }
}
