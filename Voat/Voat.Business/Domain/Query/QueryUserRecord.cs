using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryUserRecord : CachedQuery<VoatUser>
    {
        private string _userToRetrieve;

        public QueryUserRecord(string userName)
            : this(userName, new CachePolicy(TimeSpan.FromMinutes(10)))
        {
            _userToRetrieve = userName;
        }

        public QueryUserRecord(string userName, CachePolicy policy)
            : this(policy)
        {
            _userToRetrieve = userName;
        }

        public QueryUserRecord(CachePolicy policy)
            : base(policy)
        {
            _userToRetrieve = UserName;
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserRecord(_userToRetrieve);
            }
        }

        public override string CacheKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override async Task<VoatUser> GetData()
        {
            using (var db = new ApplicationDbContext())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;

                using (var context = new UserManager<VoatUser>(new UserStore<VoatUser>(db)))
                {
                    var user = await context.FindByNameAsync(_userToRetrieve);
                    return user;
                }
            }
        }
    }
}
