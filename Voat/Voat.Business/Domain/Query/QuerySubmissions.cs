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

        public QuerySubmissionsLegacy(string subverse, SearchOptions options) : this(subverse, options, CachePolicy.None)
        {
            this._options = options;
            this._subverse = subverse;
        }
        public QuerySubmissionsLegacy(string subverse, SearchOptions options, CachePolicy policy) : base(policy)
        {
            this._options = options;
            this._subverse = subverse;
        }
        //Since all submission queries will run through this we need to control caching times here
        public override CachePolicy CachingPolicy
        {
            get
            {
                if (base._cachePolicy == CachePolicy.None)
                {
                    if (_options.Sort == Models.SortAlgorithm.New)
                    {
                        return new CachePolicy(TimeSpan.FromMinutes(3));
                    }
                    else if (IsUserVolatileCache(UserName, _subverse))
                    {
                        return new CachePolicy(TimeSpan.FromMinutes(6));
                    }
                    else
                    {
                        //Want to keep first two subverse pages hotcached
                        return new CachePolicy(TimeSpan.FromMinutes(6), _options.Page <= 1 ? 3 : -1);
                    }
                }
                else
                {
                    return base.CachingPolicy;
                }
            }

            protected set
            {
                base.CachingPolicy = value;
            }
        }
        public static bool IsUserVolatileCache(string userName, string subverse)
        {
            bool result = false;
            if (!String.IsNullOrEmpty(userName))
            {
                if (
                    (subverse.IsEqual("all") || subverse.IsEqual(AGGREGATE_SUBVERSE.ALL))
                    ||
                    subverse.IsEqual(AGGREGATE_SUBVERSE.FRONT)
                   )
                {
                    result = true;
                }
            }
            return result;
        }
        public override string CacheKey
        {
            get
            {
                string userName = UserName;
                if (!IsUserVolatileCache(userName, _subverse))
                {
                    userName = "_"; //< it looks like an emoji, how cute.
                }

                return String.Format("{0}:{1}:{2}", _subverse, userName, _options.ToString(true));
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
