using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseModerators : CachedQuery<IEnumerable<SubverseModerator>>
    {
        private string _subverse;

        public QuerySubverseModerators(string subverse) : base(new CachePolicy(TimeSpan.FromHours(10)))
        {
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get { throw new NotImplementedException(); }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.SubverseModerators(_subverse);
            }
        }

        protected override async Task<IEnumerable<SubverseModerator>> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetSubverseModerators(_subverse);
            }
        }
    }
}
