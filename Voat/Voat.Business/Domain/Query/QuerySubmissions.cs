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
        private bool _populateUserData = true;

        protected DomainReference _domainReference;

        //protected string _name;
        //protected DomainType _type;
        //protected string _contextUserName;

        public QuerySubmissions(string name, DomainType type, SearchOptions options, string userName = null) : this(name, type, options, CachePolicy.None, userName)
        {
            
        }
        public QuerySubmissions(string name, DomainType type, SearchOptions options,  CachePolicy policy, string userName = null) : base(policy)
        {
            this._options = options;
            this._domainReference = new DomainReference(type, name, userName);

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
                    else if (IsUserVolatileCache(UserName, _domainReference))
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
        public static bool IsUserVolatileCache(string userName, DomainReference domainReference)
        {
            bool result = false;
            if (domainReference.Type == DomainType.Subverse)
            {
                if (!String.IsNullOrEmpty(userName))
                {
                    if (
                        (domainReference.Name.IsEqual("all") || domainReference.Name.IsEqual(AGGREGATE_SUBVERSE.ALL))
                        ||
                        domainReference.Name.IsEqual(AGGREGATE_SUBVERSE.FRONT)
                       )
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
        public override string CacheKey
        {
            get
            {
                if (_domainReference.Type == DomainType.Subverse)
                {
                    string userName = UserName;
                    if (!IsUserVolatileCache(userName, _domainReference))
                    {
                        userName = "_"; //< it looks like an emoji, how cute.
                    }
                    return String.Format("{0}:{1}:{2}:{3}", _domainReference.Type, _domainReference.Name, userName, _options.ToString(true));
                }
                else
                {
                    string userName = "_";
                    if (!String.IsNullOrEmpty(_domainReference.OwnerName))
                    {
                        userName = _domainReference.OwnerName;
                    }

                    return String.Format("{0}:{1}:{2}:{3}", _domainReference.Type, _domainReference.Name, userName, _options.ToString(true));
                }
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
                var result = await db.GetSubmissionsDapper(this._domainReference, this._options).ConfigureAwait(false);
                return result.Map();
            }
        }
    }
}
