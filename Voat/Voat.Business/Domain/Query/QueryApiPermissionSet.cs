using Newtonsoft.Json;
using System;
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

        protected override ApiPermissionSet GetData()
        {
            if (_id.HasValue && _id.Value > 0)
            {
                using (var repo = new Repository())
                {
                    var dbPolicy = repo.GetApiPermissionPolicy(_id.Value);
                    return JsonConvert.DeserializeObject<ApiPermissionSet>(dbPolicy.Policy);
                }
            }
            //return default policy
            //TODO: Change these defaults when moving to production
            //return new ApiPermissionSet() { AllowStream = false, AllowUnrestrictedLogin = false, RequireHmacOnLogin = false };
            return new ApiPermissionSet() { AllowStream = true, AllowUnrestrictedLogin = false, RequireHmacOnLogin = false };
        }
    }
}
