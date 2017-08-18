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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserBlocks : CachedQuery<IList<BlockedItem>>
    {
        private string _userName;

        public QueryUserBlocks(string userName = null) : base(new Caching.CachePolicy(TimeSpan.FromMinutes(30)))
        {
            _userName = userName;
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
        public override string UserName => String.IsNullOrEmpty(_userName) ? base.UserName : _userName;

        protected override async Task<IList<BlockedItem>> GetData()
        {
            using (var db = new Repository(User))
            {
                var blockedSubs = await db.GetBlockedSubverses(UserName);
                var blockedUsers = db.GetBlockedUsers(UserName);
                var blocked = blockedSubs.Concat(blockedUsers).ToList();
                return blocked;
            }
        }
    }
}
