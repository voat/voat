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


    public class QueryUserVotesReceived : CachedQuery<Statistics<IEnumerable<UserVoteReceivedStats>>>
    {
        protected SearchOptions _options;

        public QueryUserVotesReceived() : this(null)
        {
            //Default options
            var options = new SearchOptions();
            options.EndDate = Repository.CurrentDate.ToStartOfDay();
            options.Span = Domain.Models.SortSpan.Week;
            options.Count = 5;
            this._options = options;
        }

        public QueryUserVotesReceived(SearchOptions options) : this(options, new CachePolicy(TimeSpan.FromHours(12)))
        {

        }

        public QueryUserVotesReceived(SearchOptions options, CachePolicy policy) : base(policy)
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
                return CachingKey.Statistics.UserVotesReceived(_options);
            }
        }

        protected override async Task<Statistics<IEnumerable<UserVoteReceivedStats>>> GetData()
        {
            using (var repo = new Repository())
            {
                var result = await repo.UserVotesReceivedStatistics(this._options);
                return result;
            }
        }
    }

}
