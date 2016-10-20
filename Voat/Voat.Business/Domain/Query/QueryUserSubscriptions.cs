using System;
using System.Collections.Generic;
using System.Linq;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryUserSubscriptions : CachedQuery<IDictionary<string, IEnumerable<string>>>
    {
        private string _userToRetrieve;

        public QueryUserSubscriptions(string userToRetrieve) : this(userToRetrieve, new CachePolicy(TimeSpan.FromMinutes(15)))
        {
        }

        public QueryUserSubscriptions(string userToRetrieve, CachePolicy policy) : base(policy)
        {
            this._userToRetrieve = userToRetrieve;
        }

        public override string CacheKey
        {
            get
            {
                return _userToRetrieve;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserSubscriptions(_userToRetrieve);
            }
        }

        protected override IDictionary<string, IEnumerable<string>> GetData()
        {
            using (var db = new Repository())
            {
                var data = db.GetSubscriptions(_userToRetrieve);
                var dict = data.Select(x => x.Type).Distinct().ToDictionary(x => x.ToString(), y => data.Where(x => x.Type == y).Select(x => x.Name).ToList().AsEnumerable());
                return dict;
            }
        }
    }
}
