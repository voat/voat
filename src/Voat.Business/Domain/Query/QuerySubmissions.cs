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
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Domain.Query
{
    public class QuerySubmissions : CachedQuery<IEnumerable<Domain.Models.Submission>>
    {
        protected SearchOptions _options;
        private bool _populateUserData = true;

        protected DomainReference _domainReference;

       
        public QuerySubmissions(DomainReference domainReference, SearchOptions options) : this(domainReference, options, CachePolicy.None)
        {

        }
        public QuerySubmissions(DomainReference domainReference, SearchOptions options, CachePolicy policy) : base(policy)
        {
            this._options = options;
            this._domainReference = domainReference;

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
                        //Want to keep first two subverse pages hot cached
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
            DomainMaps.HydrateUserData(User, result);
            return result;
        }
        protected override async Task<IEnumerable<Domain.Models.Submission>> GetData()
        {
            using (var db = new Repository(User))
            {
                var result = await db.GetSubmissionsDapper(this._domainReference, this._options).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return result.Map();
            }
        }
    }
}
