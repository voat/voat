using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QuerySubmissions : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        protected SearchOptions _options;
        protected string _subverse;

        public QuerySubmissions(string subverse, SearchOptions options, CachePolicy policy = null) : base(policy)
        {
            this._options = options;
            this._subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}-{1}&userName={2}", _subverse, _options.ToString(), UserName ?? "default");
            }
        }

        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository())
            {
                var result = db.GetSubmissions(this._subverse, this._options);
                return result.Map();
            }
        }
    }
}
