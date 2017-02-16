using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QuerySubmissions : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        protected SearchOptions _options;
        protected string _name;
        protected DomainType _type;
        private bool _populateUserData = true;
        protected string _contextUserName;

        public QuerySubmissions(string name, DomainType type, SearchOptions options, string userName = null) : this(name, type, options, CachePolicy.None, userName)
        {
            
        }
        public QuerySubmissions(string name, DomainType type, SearchOptions options,  CachePolicy policy, string userName = null) : base(policy)
        {
            this._options = options;
            this._name = name;
            this._type = type;
            this._contextUserName = userName;
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
                    else if (IsUserVolatileCache(UserName, _name))
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
                if (!IsUserVolatileCache(userName, _name))
                {
                    userName = "_"; //< it looks like an emoji, how cute.
                }

                return String.Format("{0}:{1}:{2}", _name, userName, _options.ToString(true));
            }
        }

        public bool PopulateUserData
        {
            get
            {
                return _populateUserData;
            }

            set
            {
                _populateUserData = value;
            }
        }
        public override async Task<IEnumerable<Submission>> ExecuteAsync()
        {
            var result = await base.ExecuteAsync();
            DomainMaps.HydrateUserData(result);
            return result;
        }
        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository())
            {
                var result = await db.GetSubmissionsDapper(this._name, this._type, this._options, this._contextUserName).ConfigureAwait(false);
                return result.Map();
            }
        }
    }
}
