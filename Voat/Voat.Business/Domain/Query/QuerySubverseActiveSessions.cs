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


    public class QueryActiveSessionCount : CachedQuery<int>
    {
        protected DomainReference _domainReference;

        public QueryActiveSessionCount(DomainReference domainReference) : this(domainReference, new CachePolicy(TimeSpan.FromMinutes(2)))
        {

        }

        public QueryActiveSessionCount(DomainReference domainReference, CachePolicy policy) : base(policy)
        {
            this._domainReference = domainReference;
        }

        public override string CacheKey
        {
            get
            {
                return "";
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.ActiveSessionCount(_domainReference);
            }
        }

        public override async Task<int> ExecuteAsync()
        {
            var submission = await base.ExecuteAsync();
            
            return submission;
        }

        protected override async Task<int> GetData()
        {
            using (var db = new Repository(User))
            {
                var result = await db.ActiveSessionCount(this._domainReference);
                return result;
            }
        }
    }
    
}
