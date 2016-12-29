using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

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
            DomainMaps.HydrateUserData(result);
            return result;
        }
        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository())
            {
                var result = await db.GetSubmissionsByDomain(this._domain, this._options).ConfigureAwait(false);
                return result.Map();
            }
        }
    }
}
