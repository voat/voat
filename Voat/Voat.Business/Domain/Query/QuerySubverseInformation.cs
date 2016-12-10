using System;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseInformation : CachedQuery<SubverseInformation>
    {
        private string _subverse;

        public QuerySubverseInformation(string subverse) : base(new CachePolicy(TimeSpan.FromMinutes(10)))
        {
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return _subverse;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.SubverseInformation(_subverse);
            }
        }

        protected override async Task<SubverseInformation> GetData()
        {
            using (var db = new Repository())
            {
                var info = db.GetSubverseInfo(_subverse, true).Map();

                if (info != null)
                {
                    var q = new QuerySubverseModerators(_subverse);
                    var results = await q.ExecuteAsync();
                    info.Moderators = results.Select(x => x.UserName).ToList();
                }
                return info;
            }
        }
    }
}
