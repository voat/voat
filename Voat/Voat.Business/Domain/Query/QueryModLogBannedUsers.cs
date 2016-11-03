using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{

    public class QueryModLogBannedUsers : QuerySubverseBase<IEnumerable<Domain.Models.SubverseBan>>
    {
        public QueryModLogBannedUsers(string subverse, SearchOptions options) : base(subverse, options)
        {

        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.ModLogBannedUsers(_subverse, _options);
            }
        }

        protected override async Task<IEnumerable<Domain.Models.SubverseBan>> GetData()
        {
            using (var repo = new Repository())
            {
                return await repo.GetModLogBannedUsers(_subverse, _options);
            }
        }
    }
}
