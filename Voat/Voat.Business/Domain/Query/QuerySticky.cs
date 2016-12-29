using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryStickies : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        private string _subverse;

        public QueryStickies(string subverse) : base(new CachePolicy(TimeSpan.FromMinutes(30), 2))
        {
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return _subverse;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.StickySubmission(_subverse);
            }
        }
        public override async Task<IEnumerable<Submission>> ExecuteAsync()
        {
            var data = await base.ExecuteAsync();
            DomainMaps.HydrateUserData(data);
            return data;
        }
        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository())
            {
                var sticky = await db.GetSticky(_subverse);
                if (sticky != null)
                {
                    return new List<Domain.Models.Submission>() { sticky };
                }
                //return empty list
                return new List<Domain.Models.Submission>();
            }
        }
    }
}
