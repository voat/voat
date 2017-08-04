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
    public class QueryStickies : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        private string _subverse;

        public QueryStickies(string subverse) : base(new CachePolicy(TimeSpan.FromMinutes(30), 2))
        {
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return _subverse;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.StickySubmission(_subverse);
            }
        }
        public override async Task<IEnumerable<Submission>> ExecuteAsync()
        {
            var data = await base.ExecuteAsync();
            DomainMaps.HydrateUserData(User, data);
            return data;
        }
        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository(User))
            {
                var stickies = await db.GetStickies(_subverse);
                if (stickies != null)
                {
                    return stickies;
                }
                //return empty list
                return new List<Domain.Models.Submission>();
            }
        }
    }
}
