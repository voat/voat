using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Domain.Query.Statistics
{


    public class QueryUserVotesGiven : CachedQuery<Statistics<IEnumerable<UserVoteStats>>>
    {
        protected SearchOptions _options;

        public QueryUserVotesGiven() : this(null)
        {
            //Default options
            var options = new SearchOptions();
            options.EndDate = Repository.CurrentDate.ToStartOfDay();
            options.Span = Domain.Models.SortSpan.Week;
            options.Count = 5;
            this._options = options;
        }

        public QueryUserVotesGiven(SearchOptions options) : this(options, new CachePolicy(TimeSpan.FromHours(12)))
        {

        }

        public QueryUserVotesGiven(SearchOptions options, CachePolicy policy) : base(policy)
        {
            this._options = options;
        }

        public override string CacheKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.Statistics.UserVotesGiven(_options);
            }
        }

        protected override async Task<Statistics<IEnumerable<UserVoteStats>>> GetData()
        {
            using (var db = new Repository())
            {
                var result = await db.UserVotesGivenStatistics(this._options);
                return result;
            }
        }
    }
    
}
