using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryDefaultSubverses : CachedQuery<IEnumerable<SubverseInformation>>
    {
        protected int _commentID;

        public QueryDefaultSubverses() : this(new CachePolicy(TimeSpan.FromMinutes(60)))
        {
        }

        public QueryDefaultSubverses(CachePolicy policy) : base(policy)
        {
        }

        public override string CacheKey
        {
            get
            {
                return "DefaultSubverses";
            }
        }

        protected override async Task<IEnumerable<SubverseInformation>> GetData()
        {
            using (var db = new Repository())
            {
                return db.GetDefaultSubverses();
            }
        }
    }
}
