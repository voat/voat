using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserSubscriptions : CachedQuery<IDictionary<DomainType, IEnumerable<string>>>
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

        protected override async Task<IDictionary<DomainType, IEnumerable<string>>> GetData()
        {
            using (var db = new Repository())
            {
                var data = db.GetSubscriptions(_userToRetrieve);
                var dict = new Dictionary<DomainType, IEnumerable<string>>();

                var type = DomainType.Subverse;
                dict.Add(type, data.Where(x => x.Type == type).Select(x => x.Name).ToList());

                type = DomainType.Set;
                dict.Add(type, data.Where(x => x.Type == type).Select(x => x.Name).ToList());
                //var dict = data.Select(x => x.Type).Distinct().ToDictionary(x => x.ToString(), y => data.Where(x => x.Type == y).Select(x => x.Name).ToList().AsEnumerable());
                return dict;
            }
        }
    }
}
