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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserSaves : Query<ISet<int>>
    {
        private ContentType _type;

        public QueryUserSaves(ContentType type) : base()
        {
            _type = type;
        }

        public override async Task<ISet<int>> ExecuteAsync()
        {
            DemandAuthentication();
            
            var handler = CacheHandler.Instance;
            if (handler.CacheEnabled)
            {
                var cacheKey = CachingKey.UserSavedItems(_type, User.Identity.Name);
                if (!handler.Exists(cacheKey))
                {
                    handler.Replace(cacheKey, await GetData(), TimeSpan.FromMinutes(30));
                }
                return new CacheSetAccessor<int>(cacheKey);
            }
            else
            {
                return await GetData();
            }
        }

        protected async Task<ISet<int>> GetData()
        {
            DemandAuthentication();

            using (var repo = new Repository(User))
            {
                var val = await repo.GetUserSavedItems(_type, User.Identity.Name);
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
