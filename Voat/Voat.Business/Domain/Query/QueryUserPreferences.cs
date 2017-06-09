#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

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
    public class QueryUserPreferences : CachedQuery<Domain.Models.UserPreference>
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

        protected override async Task<Domain.Models.UserPreference> GetData()
        {
            UserPreference pref = null;

            using (var db = new Repository(User))
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
            return pref.Map();
        }
    }
}
