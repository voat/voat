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
using Voat.Data;
using Voat.Data.Models;
using Voat.Utilities;

namespace Voat.Domain.Query
{
    public class QueryUserApiKeys : Query<IEnumerable<Tuple<ApiClient, ApiThrottlePolicy, Domain.Models.ApiPermissionSet>>>
    {
        public override async Task<IEnumerable<Tuple<ApiClient, ApiThrottlePolicy, Domain.Models.ApiPermissionSet>>> ExecuteAsync()
        {
            var output = new List<Tuple<ApiClient, ApiThrottlePolicy, Domain.Models.ApiPermissionSet>>();
            using (var repo = new Repository(User))
            {
                var keys = repo.GetApiKeys(UserName);
                foreach (var key in keys)
                {
                    ApiThrottlePolicy policy = null;
                    Domain.Models.ApiPermissionSet perms = null;
                    if (key.ApiThrottlePolicyID.HasValue)
                    {
                        var q = new QueryApiThrottlePolicy(key.ApiThrottlePolicyID.Value);
                        policy = await q.ExecuteAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    }
                    var p = new QueryApiPermissionSet(key.ApiPermissionPolicyID);
                    perms = await p.ExecuteAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                    var tuple = Tuple.Create(key, policy, perms);
                    output.Add(tuple);
                }
            }
            return output;
        }
    }
}
