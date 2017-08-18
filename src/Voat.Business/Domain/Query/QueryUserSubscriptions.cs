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
    public class QueryUserSubscriptions : CachedQuery<IDictionary<DomainType, IEnumerable<string>>>
    {
        private string _userToRetrieve;

        public QueryUserSubscriptions(string userToRetrieve) : this(userToRetrieve, new CachePolicy(TimeSpan.FromMinutes(15)))
        {
        }

        public QueryUserSubscriptions(string userToRetrieve, CachePolicy policy) : base(policy)
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
                return CachingKey.UserSubscriptions(_userToRetrieve);
            }
        }

        protected override async Task<IDictionary<DomainType, IEnumerable<string>>> GetData()
        {
            using (var db = new Repository(User))
            {
                var data = db.GetSubscriptions(_userToRetrieve);
                var dict = new Dictionary<DomainType, IEnumerable<string>>();

                var type = DomainType.Subverse;
                dict.Add(type, data.Where(x => x.Type == type).Select(x => x.FullName).OrderBy(x => x).ToList());

                type = DomainType.Set;
                dict.Add(type, data.Where(x => x.Type == type).Select(x => x.FullName).OrderBy(x => x).ToList());
                //var dict = data.Select(x => x.Type).Distinct().ToDictionary(x => x.ToString(), y => data.Where(x => x.Type == y).Select(x => x.Name).ToList().AsEnumerable());
                return dict;
            }
        }
    }
}
