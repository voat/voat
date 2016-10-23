using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryAPIKey : CachedQuery<ApiClient>
    {
        private string _apikey = null;

        public QueryAPIKey(string apiKey) : base(new CachePolicy(TimeSpan.FromMinutes(10)))
        {
            _apikey = apiKey;
        }

        public override string CacheKey
        {
            get
            {
                return _apikey;
            }
        }

        protected override string FullCacheKey
        {
            get { return CachingKey.ApiClient(_apikey); }
        }

        protected override async Task<ApiClient> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetApiKey(_apikey);
            }
        }
    }
}
