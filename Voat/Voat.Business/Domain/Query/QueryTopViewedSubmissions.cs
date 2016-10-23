using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryTopViewedSubmissions : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        public QueryTopViewedSubmissions() : base(new Caching.CachePolicy(TimeSpan.FromMinutes(15), 0)) //Recache this everytime it expires
        {
        }

        public override string CacheKey
        {
            get
            {
                return "TopViewed";
            }
        }

        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var repo = new Repository())
            {
                return repo.GetTopViewedSubmissions().Map();
            }
        }
    }
}
