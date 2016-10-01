using System;
using System.Collections.Generic;
using System.Linq;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseUserBans : CachedQuery<IList<SubverseBan>>
    {
        private string _subverse;

        public QuerySubverseUserBans(string subverse) : base(new CachePolicy(TimeSpan.FromHours(10)))
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
                return CachingKey.SubverseUserBans(_subverse);
            }
        }

        protected override IList<SubverseBan> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetSubverseUserBans(_subverse).ToList();
            }
        }
    }
}
