using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryVotes : CachedQuery<IEnumerable<Domain.Models.Vote>>
    {
        public override string CacheKey => throw new NotImplementedException();

        public QueryVotes() : base(new Caching.CachePolicy(TimeSpan.FromMinutes(60)))
        {

        }

        protected override Task<IEnumerable<Vote>> GetData()
        {
            throw new NotImplementedException();
        }
    }
}
