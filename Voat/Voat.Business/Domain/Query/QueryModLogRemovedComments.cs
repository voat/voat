using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryModLogRemovedComments : QuerySubverseBase<IEnumerable<Domain.Models.CommentRemovalLog>>
    {
        public QueryModLogRemovedComments(string subverse, SearchOptions options) : base(subverse, options)
        {

        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.ModLogComments(_subverse, _options);
            }
        }

        protected override async Task<IEnumerable<Domain.Models.CommentRemovalLog>> GetData()
        {
            using (var repo = new Repository())
            {
                return await repo.GetModLogRemovedComments(_subverse, _options);
            }
        }
    }
}
