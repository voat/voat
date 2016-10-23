using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QueryUserApiKeys : Query<IEnumerable<Tuple<ApiClient, ApiThrottlePolicy>>>
    {
        public override async Task<IEnumerable<Tuple<ApiClient, ApiThrottlePolicy>>> ExecuteAsync()
        {
            var output = new List<Tuple<ApiClient, ApiThrottlePolicy>>();
            using (var repo = new Repository())
            {
                var keys = repo.GetApiKeys(UserName);
                foreach (var key in keys)
                {
                    ApiThrottlePolicy policy = null;
                    if (key.ApiThrottlePolicyID.HasValue)
                    {
                        var q = new QueryApiThrottlePolicy(key.ApiThrottlePolicyID.Value);
                        policy = await q.ExecuteAsync();
                    }

                    var tuple = new Tuple<ApiClient, ApiThrottlePolicy>(key, policy);
                    output.Add(tuple);
                }
            }
            return output;
        }
    }
}
