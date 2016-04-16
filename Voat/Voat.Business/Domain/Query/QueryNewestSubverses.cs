using System;
using System.Collections.Generic;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryNewestSubverses : CachedQuery<IEnumerable<SubverseInformation>>
    {
        private string _phrase;

        public QueryNewestSubverses() : this(new CachePolicy(TimeSpan.FromHours(1)))
        {
        }

        public QueryNewestSubverses(CachePolicy policy) : base(policy)
        {
        }

        public override string CacheKey
        {
            get
            {
                return "New";
            }
        }

        protected override IEnumerable<SubverseInformation> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetNewestSubverses();
            }
        }
    }
}
