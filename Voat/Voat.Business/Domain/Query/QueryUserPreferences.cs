using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    /// <summary>
    /// Returns the user profile. If no user is logged in return a default profile.
    /// </summary>
    public class QueryUserPreferences : CachedQuery<UserPreference>
    {
        private string _userToRetrieve;

        public QueryUserPreferences(string userName)
            : this()
        {
            _userToRetrieve = userName;
        }

        public QueryUserPreferences()
            : this(new CachePolicy(TimeSpan.FromMinutes(30)))
        {
        }

        public QueryUserPreferences(CachePolicy policy) : base(policy)
        {
            _userToRetrieve = UserName;
        }

        public override string CacheKey
        {
            get
            {
                return (String.IsNullOrEmpty(_userToRetrieve) ? "_default" : _userToRetrieve);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserPreferences(CacheKey);
            }
        }

        protected override async Task<UserPreference> GetData()
        {
            UserPreference pref = null;

            using (var db = new Repository())
            {
                pref = await db.GetUserPreferences(_userToRetrieve);
            }
            //REPO now handles
            ////User doesn't have prefs or user is is not logged in. Create default pref and return
            //if (pref == null)
            //{
            //    pref = new UserPreference();
            //    Repository.SetDefaultUserPreferences(pref);
            //    pref.UserName = _userToRetrieve;
            //}
            return pref;
        }
    }
}
