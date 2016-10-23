using System;
using System.Threading.Tasks;
using Voat.Caching;

namespace Voat.Domain.Query
{
    [Obsolete("No longer going to cache UserData")]
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

        protected override async Task<UserData> GetData()
        {
            try
            {
                return new UserData(_userToRetrieve, true);
            }
            catch (Exception ex)
            {
                //throw new VoatNotFoundException($"Can not find user record for {_userToRetrieve}");
                return null;
            }
        }
    }
}
