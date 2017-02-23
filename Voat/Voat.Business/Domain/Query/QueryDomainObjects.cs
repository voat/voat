using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryDomainObject : CachedQuery<IEnumerable<DomainReferenceDetails>>
    {
        protected DomainType _domainType;
        protected Data.SearchOptions _options;

        public QueryDomainObject(DomainType domainType, Data.SearchOptions options) : base(new CachePolicy(TimeSpan.FromHours(1)))
        {
            _domainType = domainType;
            _options = options;
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
            get { return CachingKey.DomainObjectSearch(_domainType, _options); }
        }

        protected override async Task<IEnumerable<DomainReferenceDetails>> GetData()
        {
            using (var repo = new Repository())
            {
                var results = await repo.SearchDomainObjects(_domainType, _options);
                return results;
            }
        }
    }
}