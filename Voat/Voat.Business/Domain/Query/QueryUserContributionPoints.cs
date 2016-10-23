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
    public class QueryUserContributionPoints : CachedQuery<Score>
    {
        private ContentType _type;
        private string _subverse;

        public QueryUserContributionPoints(ContentType type, string subverse)
            : base(new CachePolicy(Caching.CachingTimeSpan.UserData))
        {
            this._type = type;
            this._subverse = subverse;
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
                return Caching.CachingKey.UserContributionPointsForSubverse(UserName, _type, _subverse);
            }
        }
        protected override async Task<Score> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.UserContributionPoints(UserName, _type, _subverse);
            }
        }
    }
}
