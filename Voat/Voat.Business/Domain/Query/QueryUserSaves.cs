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
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
  
    public class QueryUserSaves : CachedQuery<ISet<int>>
    {
        private ContentType _type;

        public QueryUserSaves(ContentType type) : base(new CachePolicy(CachingTimeSpan.UserData))
        {
            _type = type;
        }

        public override string CacheKey
        {
            get { throw new NotImplementedException(); }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserSavedItems(_type, UserName);
            }
        }
        protected override async Task<ISet<int>> GetData()
        {
            using (var repo = new Repository())
            {
                var val = await repo.GetUserSavedItems(_type, UserName);
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
