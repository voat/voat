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
using Voat.Utilities;

namespace Voat.Domain.Query
{
    public class QuerySubmissionsByDomain : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        protected SearchOptions _options;
        protected string _domain;
        
        public QuerySubmissionsByDomain(string domain, SearchOptions options) : this(domain, options, new CachePolicy(TimeSpan.FromMinutes(60)))
        {
            this._options = options;
            this._domain = domain;
        }
        public QuerySubmissionsByDomain(string domain, SearchOptions options, CachePolicy policy) : base(policy)
        {
            this._options = options;
            this._domain = domain;
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
                return CachingKey.DomainSearch(_domain, _options.Page, _options.Sort.ToString());
            }
        }
        
        public override async Task<IEnumerable<Domain.Models.Submission>> ExecuteAsync()
        {
            var result = await base.ExecuteAsync();
            DomainMaps.HydrateUserData(User, result);
            return result;
        }
        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository(User))
            {
                var result = await db.GetSubmissionsByDomain(this._domain, this._options).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return result.Map();
            }
        }
    }
}
