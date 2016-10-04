using System;
using System.Collections.Generic;
using System.Linq;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserBlocks : CachedQuery<IList<BlockedItem>>
    {

        public QueryUserBlocks(string userName = null) : base(new Caching.CachePolicy(TimeSpan.FromMinutes(30)))
        {
            userName = (String.IsNullOrEmpty(userName) ? UserName : userName);
        }

        public override string CacheKey
        {
            get
            {
                return UserName;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserBlocks(UserName);
            }
        }

        protected override IList<BlockedItem> GetData()
        {
            using (var db = new Repository())
            {
                var blockedSubs = db.GetBlockedSubverses(UserName);
                var blockedUsers = db.GetBlockedUsers(UserName);
                var blocked = blockedSubs.Concat(blockedUsers).ToList();
                return blocked;
            }
        }
    }
}
