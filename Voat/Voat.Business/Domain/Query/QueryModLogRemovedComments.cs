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
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryModLogRemovedComments : QuerySubverseBase<IEnumerable<Domain.Models.CommentRemovalLog>>
    {
        public QueryModLogRemovedComments(string subverse, SearchOptions options) : base(subverse, options)
        {

        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.ModLogComments(_subverse, _options);
            }
        }

        protected override async Task<IEnumerable<Domain.Models.CommentRemovalLog>> GetData()
        {
            using (var repo = new Repository(User))
            {
                return await repo.GetModLogRemovedComments(_subverse, _options);
            }
        }
    }
}
