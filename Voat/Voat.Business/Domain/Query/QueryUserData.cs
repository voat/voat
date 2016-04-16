using System;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;
using System.Linq;

namespace Voat.Domain.Query
{
    public class QueryUserData : CachedQuery<UserData>
    {
        private string _userToRetrieve;

        public QueryUserData(string userToRetrieve) : this(userToRetrieve, new CachePolicy(TimeSpan.FromMinutes(10)))
        {
        }

        public QueryUserData(string userToRetrieve, CachePolicy policy) : base(policy)
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
                return CachingKey.UserData(_userToRetrieve);
            }
        }

        protected override UserData GetData()
        {
            return new UserData(_userToRetrieve);
        }
    }
}
