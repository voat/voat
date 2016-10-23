using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserBlocks : CachedQuery<IList<BlockedItem>>
    {
        private string _userName;

        public QueryUserBlocks(string userName = null) : base(new Caching.CachePolicy(TimeSpan.FromMinutes(30)))
        {
            _userName = (String.IsNullOrEmpty(userName) ? UserName : userName);
        }

        public override string CacheKey
        {
            get
            {
                return _userName;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserBlocks(_userName);
            }
        }

        protected override async Task<IList<BlockedItem>> GetData()
        {
            using (var db = new Repository())
            {
                var blockedSubs = db.GetBlockedSubverses(_userName);
                var blockedUsers = db.GetBlockedUsers(_userName);
                var blocked = blockedSubs.Concat(blockedUsers).ToList();
                return blocked;
            }
        }
    }
}
