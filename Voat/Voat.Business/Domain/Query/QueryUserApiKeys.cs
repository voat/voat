using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryUserApiKeys : Query<IEnumerable<Tuple<ApiClient, ApiThrottlePolicy, Domain.Models.ApiPermissionSet>>>
    {
        public override async Task<IEnumerable<Tuple<ApiClient, ApiThrottlePolicy, Domain.Models.ApiPermissionSet>>> ExecuteAsync()
        {
            var output = new List<Tuple<ApiClient, ApiThrottlePolicy, Domain.Models.ApiPermissionSet>>();
            using (var repo = new Repository())
            {
                var keys = repo.GetApiKeys(UserName);
                foreach (var key in keys)
                {
                    ApiThrottlePolicy policy = null;
                    Domain.Models.ApiPermissionSet perms = null;
                    if (key.ApiThrottlePolicyID.HasValue)
                    {
                        var q = new QueryApiThrottlePolicy(key.ApiThrottlePolicyID.Value);
                        policy = await q.ExecuteAsync().ConfigureAwait(false);
                    }
                    var p = new QueryApiPermissionSet(key.ApiPermissionPolicyID);
                    perms = await p.ExecuteAsync().ConfigureAwait(false);

                    var tuple = Tuple.Create(key, policy, perms);
                    output.Add(tuple);
                }
            }
            return output;
        }
    }
}
