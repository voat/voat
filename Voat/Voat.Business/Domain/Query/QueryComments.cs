using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryComments : CachedQuery<IEnumerable<Domain.Models.Comment>>
    {
        protected SearchOptions _options;
        protected string _subverse;

        public QueryComments(string subverse, SearchOptions options, CachePolicy policy = null) : base(policy)
        {
            this._options = options;
            this._subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return _options.ToString();
            }
        }

        protected override async Task<IEnumerable<Domain.Models.Comment>> GetData()
        {
            using (var db = new Repository())
            {
                var result = db.GetComments(_subverse, this._options);
                return Domain.DomainMaps.Map(result);
            }
        }
    }
}
