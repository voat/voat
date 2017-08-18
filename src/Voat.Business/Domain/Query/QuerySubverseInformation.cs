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
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseInformation : CachedQuery<SubverseInformation>
    {
        private string _subverse;

        public QuerySubverseInformation(string subverse) : base(new CachePolicy(TimeSpan.FromMinutes(10)))
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
                return CachingKey.SubverseInformation(_subverse);
            }
        }

        protected override async Task<SubverseInformation> GetData()
        {
            using (var db = new Repository(User))
            {
                var info = db.GetSubverseInfo(_subverse, true).Map();

                if (info != null)
                {
                    var q = new QuerySubverseModerators(_subverse);
                    var results = await q.ExecuteAsync();
                    info.Moderators = results.Select(x => x.UserName).ToList();
                }
                return info;
            }
        }
    }
}
