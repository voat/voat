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
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverseSearch : CachedQuery<IEnumerable<SubverseInformation>>
    {
        private string _phrase;

        public QuerySubverseSearch(string phrase) : this(phrase, new CachePolicy(TimeSpan.FromHours(1)))
        {
        }

        public QuerySubverseSearch(string phrase, CachePolicy policy) : base(policy)
        {
            _phrase = phrase;
        }

        public override string CacheKey
        {
            get
            {
                //TODO: Should hash this
                return _phrase;
            }
        }

        protected override async Task<IEnumerable<SubverseInformation>> GetData()
        {
            using (var repo = new Repository(User))
            {
                return repo.FindSubverses(_phrase);
            }
        }
    }
}
