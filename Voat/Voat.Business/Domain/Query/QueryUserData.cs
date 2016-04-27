using System;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Voat.Data.Models;

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
            if (!String.IsNullOrEmpty(_userToRetrieve))
            {
                using (var repo = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
                {
                    var user = repo.FindByName(_userToRetrieve);
                    if (user != null)
                    {
                        return new UserData(_userToRetrieve);
                    }
                }
            }
            return null;
        }
    }
}
