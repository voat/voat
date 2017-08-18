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
    public class QueryUserCommentVotesForSubmission : CachedQuery<IEnumerable<VotedValue>>
    {
        protected int _submissionID;

        public QueryUserCommentVotesForSubmission(int submissionID) : this(submissionID, new CachePolicy(TimeSpan.FromMinutes(5)))
        {
        }

        public QueryUserCommentVotesForSubmission(int submissionID, CachePolicy policy) : base(policy)
        {
            this._submissionID = submissionID;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}:{1}", UserName, _submissionID);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserCommentVotes(UserName, _submissionID);
            }
        }

        protected override async Task<IEnumerable<VotedValue>> GetData()
        {
            using (var repo = new Repository(User))
            {
                var result = repo.UserCommentVotesBySubmission(UserName, _submissionID);
                return (result == null || !result.Any() ? null : result);
            }
        }
    }
   
}
