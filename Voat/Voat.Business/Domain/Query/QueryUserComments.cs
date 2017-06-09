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
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{

    public abstract class CachedQuerySearch<T> : CachedQuery<T>
    {
        private SearchOptions _searchOptions;

        public CachedQuerySearch(SearchOptions options, CachePolicy policy) : base(policy)
        {
            this._searchOptions = options;
        }
        public SearchOptions SearchOptions
        {
            get
            {
                return _searchOptions;
            }
        }
    }

    public class QueryUserComments : CachedQuerySearch<IEnumerable<SubmissionComment>>
    {
        private string _userName;

        public QueryUserComments(string userName, SearchOptions options) 
            : base(options, new Caching.CachePolicy(TimeSpan.FromMinutes(30)))
        {
            _userName = userName;
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
                return CachingKey.UserContent(_userName, ContentType.Comment, base.SearchOptions);
            }
        }

        protected override async Task<IEnumerable<SubmissionComment>> GetData()
        {
            using (var repo = new Repository(User))
            {
                var data = await repo.GetUserComments(this._userName, this.SearchOptions);
                return data;
            }
        }
    }
    public class QueryUserSubmissions : CachedQuerySearch<IEnumerable<Domain.Models.Submission>>
    {
        private string _userName;

        public QueryUserSubmissions(string userName, SearchOptions options)
            : base(options, new Caching.CachePolicy(TimeSpan.FromMinutes(30)))
        {
            _userName = userName;
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
                return CachingKey.UserContent(_userName, ContentType.Submission, base.SearchOptions);
            }
        }

        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var repo = new Repository(User))
            {
                var data = await repo.GetUserSubmissions(null, this._userName, this.SearchOptions);
                return data.Map();
            }
        }
    }
}
