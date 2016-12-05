using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QuerySubmissions : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        protected SearchOptions _options;
        protected string _subverse;

        public QuerySubmissions(string subverse, SearchOptions options, CachePolicy policy = null) : base(policy)
        {
            this._options = options;
            this._subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}-{1}&userName={2}", _subverse, _options.ToString(), UserName ?? "default");
            }
        }

        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository())
            {
                var result = await db.GetSubmissionsDapper(this._subverse, this._options).ConfigureAwait(false);
                return result.Map();
            }
        }
    }
    //This is an incremental port because the UI Views support Data.Models.Submission and not Domain.Models.Submission. 
    //After this code is tested, this class usage and related views will be converted to handle the QuerySubmissions object
    //which returns Domain.Models.Submission objects
    public class QuerySubmissionsLegacy : CachedQuery<IEnumerable<Data.Models.Submission>>
    {
        protected SearchOptions _options;
        protected string _subverse;

        public QuerySubmissionsLegacy(string subverse, SearchOptions options) : base(CachePolicy.None)
        {
            this._options = options;
            this._subverse = subverse;
        }

        //Since all submission queries will run through this we need to control caching times here
        public override CachePolicy CachingPolicy
        {
            get
            {
                if (_options.Sort == Models.SortAlgorithm.New)
                {
                    return CachePolicy.None;
                }
                else if (IsUserSpecificCache())
                {
                    return new CachePolicy(TimeSpan.FromMinutes(5));
                }
                else
                {
                    //Want to keep first two subverse pages hotcached
                    return new CachePolicy(TimeSpan.FromMinutes(5), _options.Page <= 1 ? 3 : -1);
                }

            }

            protected set
            {
                base.CachingPolicy = value;
            }
        }
        private bool IsUserSpecificCache()
        {
            bool result = true;
            if (!_subverse.Equals("all", StringComparison.OrdinalIgnoreCase) 
                && 
                !_subverse.Equals(AGGREGATE_SUBVERSE.ALL, StringComparison.OrdinalIgnoreCase)
                &&
                !_subverse.Equals(AGGREGATE_SUBVERSE.DEFAULT, StringComparison.OrdinalIgnoreCase)
                )
            {
                result = false;
            }
            return result;
        }
        public override string CacheKey
        {
            get
            {
                string userName = UserName;
                if (!IsUserSpecificCache())
                {
                    userName = "_everyone_";
                }

                return String.Format("{0}:{1}:{2}", _subverse, userName, _options.ToString());
            }
        }

        protected override async Task<IEnumerable<Data.Models.Submission>> GetData()
        {
            using (var db = new Repository())
            {
                var result = await db.GetSubmissionsDapper(this._subverse, this._options).ConfigureAwait(false);
                return result;
                //return result.Map();
            }
        }
    }
}
