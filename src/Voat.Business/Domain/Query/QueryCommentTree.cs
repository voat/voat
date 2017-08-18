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
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    //This class exposes anon user names as it needs to determine submitter state
    internal class QueryCommentTree : CachedQuery<IDictionary<int, usp_CommentTree_Result>>
    {
        protected int _submissionID;

        public QueryCommentTree(int submissionID) : this(submissionID, new CachePolicy(TimeSpan.FromMinutes(15)))
        {
            //default constructor with cache policy specified.
        }

        public QueryCommentTree(int submissionID, CachePolicy policy = null) : base(policy)
        {
            _submissionID = submissionID;
        }

        public override string CacheKey
        {
            get
            {
                return _submissionID.ToString();
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.CommentTree(_submissionID);
            }
        }

        protected override async Task<IDictionary<int, usp_CommentTree_Result>> GetData()
        {
            using (var db = new Repository(User))
            {
                return db.GetCommentTree(this._submissionID, null, null).ToDictionary(x => x.ID);
            }
        }
    }
}
