﻿using System;
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

        public QueryUserPreferences(string userName) : this()
        {
            _userToRetrieve = UserName;
        }

        public QueryUserPreferences() : this(new CachePolicy(TimeSpan.FromMinutes(10)))
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
                return (String.IsNullOrEmpty(UserName) ? "_default" : UserName);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserPreferences(CacheKey);
            }
        }

        protected override UserPreference GetData()
        {
            UserPreference pref = null;

            if (!string.IsNullOrEmpty(_userToRetrieve))
            {
                using (var db = new Repository())
                {
                    pref = db.GetUserPreferences(_userToRetrieve);
                }
            }
            //User doesn't have prefs or user is is not logged in. Create default pref and return
            if (pref == null)
            {
                pref = new UserPreference();
                Repository.SetDefaultUserPreferences(pref);
                pref.UserName = _userToRetrieve;
            }
            return pref;
        }
    }
}
