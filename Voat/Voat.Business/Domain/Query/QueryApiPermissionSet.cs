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

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryApiPermissionSet : CachedQuery<ApiPermissionSet>
    {
        private int? _id = 0;

        public QueryApiPermissionSet(int? apiPermissionPolicy) : base(new CachePolicy(TimeSpan.FromMinutes(20)))
        {
            _id = apiPermissionPolicy;
        }

        public override string CacheKey
        {
            get { throw new NotImplementedException("This should never be called"); }
        }

        protected override string FullCacheKey
        {
            get { return CachingKey.ApiPermissionPolicy(_id.HasValue ? _id.Value : 0); }
        }

        protected override async Task<ApiPermissionSet> GetData()
        {
            if (_id.HasValue && _id.Value > 0)
            {
                using (var repo = new Repository(User))
                {
                    var dbPolicy = repo.GetApiPermissionPolicy(_id.Value);
                    var policy = JsonConvert.DeserializeObject<ApiPermissionSet>(dbPolicy.Policy);
                    policy.Name = dbPolicy.Name;
                    return policy;
                }
            }

            //return default policy
            //TODO: Change these defaults when moving to production
            //return new ApiPermissionSet() { AllowLogin = false, AllowStream = false, AllowUnrestrictedLogin = false, RequireHmacOnLogin = false };
            return new ApiPermissionSet() { Name = "Default", AllowLogin = false, AllowStream = false, AllowUnrestrictedLogin = false, RequireHmacOnLogin = false };
        }
    }
}
