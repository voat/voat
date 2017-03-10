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


    public class QueryHighestVotedContent : CachedQuery<Statistics<IEnumerable<StatContentItem>>>
    {
        protected SearchOptions _options;

        public QueryHighestVotedContent() : this(null)
        {
            //Default options
            var options = new SearchOptions();
            options.EndDate = Repository.CurrentDate.ToStartOfDay();
            options.Span = Domain.Models.SortSpan.Week;
            options.Count = 5;
            this._options = options;
        }

        public QueryHighestVotedContent(SearchOptions options) : this(options, new CachePolicy(TimeSpan.FromHours(12)))
        {

        }

        public QueryHighestVotedContent(SearchOptions options, CachePolicy policy) : base(policy)
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
                return CachingKey.Statistics.HighestVotedContent(_options);
            }
        }

        protected override async Task<Statistics<IEnumerable<StatContentItem>>> GetData()
        {
            using (var repo = new Repository())
            {
                var result = await repo.HighestVotedContentStatistics(this._options);
                return result;
            }
        }
    }

}
