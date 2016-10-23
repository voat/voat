using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
  
    public class QueryUserSaves : CachedQuery<ISet<int>>
    {
        private ContentType _type;

        public QueryUserSaves(ContentType type) : base(new CachePolicy(CachingTimeSpan.UserData))
        {
            _type = type;
        }

        public override string CacheKey
        {
            get { throw new NotImplementedException(); }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserSavedItems(_type, UserName);
            }
        }
        protected override async Task<ISet<int>> GetData()
        {
            using (var repo = new Repository())
            {
                var val = await repo.GetUserSavedItems(_type, UserName);
                var set = new HashSet<int>(val);
                //we want to ensure this is cached and if a user has no saves 
                //the redis cache will not store anything so we create a dummy entry here
                if (set.Count == 0)
                {
                    set.Add(-1);
                }
                return set;
            }
        }
    }
}
