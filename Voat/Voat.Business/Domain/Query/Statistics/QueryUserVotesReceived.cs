#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
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
            using (var repo = new Repository(User))
            {
                var result = await repo.UserVotesReceivedStatistics(this._options);
                return result;
            }
        }
    }

}
