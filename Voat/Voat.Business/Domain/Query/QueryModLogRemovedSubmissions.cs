using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryModLogRemovedSubmissions : QuerySubverseBase<IEnumerable<Data.Models.SubmissionRemovalLog>>
    {
        public QueryModLogRemovedSubmissions(string subverse, SearchOptions options) : base(subverse, options)
        {

        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.ModLogSubmissions(_subverse, _options);
            }
        }

        protected override async Task<IEnumerable<Data.Models.SubmissionRemovalLog>> GetData()
        {
            using (var repo = new Repository())
            {
                return await repo.GetModLogRemovedSubmissions(_subverse, _options);
            }
        }
    }
}
