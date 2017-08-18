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

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryUserRecord : CachedQuery<VoatIdentityUser>
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

        protected override async Task<VoatIdentityUser> GetData()
        {
            using (var db = new IdentityDataContext())
            {
                using (var context = VoatUserManager.Create())
                {
                    var user = await context.FindByNameAsync(_userToRetrieve);
                    return user;
                }
            }
        }
    }
}
