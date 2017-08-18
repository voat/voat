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


    public class QuerySubmission : CachedQuery<Domain.Models.Submission>
    {
        protected int _submissionID;
        protected bool _hydrateUserData;

        public QuerySubmission(int submissionID, bool hydrateUserData = false) : this(submissionID, hydrateUserData, new CachePolicy(TimeSpan.FromMinutes(3)))
        {

        }

        public QuerySubmission(int submissionID, bool hydrateUserData, CachePolicy policy) : base(policy)
        {
            this._submissionID = submissionID;
            this._hydrateUserData = hydrateUserData;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}", _submissionID);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.Submission(_submissionID);
            }
        }

        public override async Task<Submission> ExecuteAsync()
        {
            var submission = await base.ExecuteAsync();
            if (_hydrateUserData)
            {
                DomainMaps.HydrateUserData(User, submission);
            }
            return submission;
        }

        protected override async Task<Domain.Models.Submission> GetData()
        {
            using (var db = new Repository(User))
            {
                var result = db.GetSubmission(this._submissionID);

                //TODO: This returns submissions from disabled subs
                return result.Map();
            }
        }
    }
    
}
