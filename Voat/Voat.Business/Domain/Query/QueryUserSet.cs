using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserSet : CachedQuery<Domain.Models.UserSet>
    {
        private string _setName;

        public QueryUserSet(string setName, CachePolicy policy = null)
            : base(policy == null ? new CachePolicy(TimeSpan.FromMinutes(10)) : policy)
        {
            _setName = setName;
        }

        public override string CacheKey
        {
            get { throw new NotImplementedException(); }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserSet(_setName);
            }
        }

        protected override async Task<UserSet> GetData()
        {
            throw new NotImplementedException();
        }
    }
}
