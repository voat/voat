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
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseFlair : CachedQuery<IEnumerable<SubverseFlair>>
    {
        private string _subverse;
        public QuerySubverseFlair(string subverse) : base(new CachePolicy(TimeSpan.FromDays(1)))
        {
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.SubverseFlair(_subverse);
            }
        }

        protected override async Task<IEnumerable<SubverseFlair>> GetData()
        {
            using (var repo = new Repository(User))
            {
                return await repo.GetSubverseFlair(_subverse);
            }
        }
    }
}
