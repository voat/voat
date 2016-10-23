using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryTopSubscribedSubverses : CachedQuery<IEnumerable<SubverseInformation>>
    {
        private string _phrase;

        public QueryTopSubscribedSubverses() : this(new CachePolicy(TimeSpan.FromHours(5)))
        {
        }

        public QueryTopSubscribedSubverses(CachePolicy policy) : base(policy)
        {
        }

        public override string CacheKey
        {
            get
            {
                return "Top";
            }
        }

        protected override async Task<IEnumerable<SubverseInformation>> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetTopSubscribedSubverses();
            }
        }
    }
}
