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
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Domain.Query
{
    public class QueryApiStreamLastCalledDate : CachedQuery<DateTime>
    {
        private ContentType _contentType = ContentType.Submission;
        private string _subverse = null;
        private static TimeSpan _timeSpan = TimeSpan.FromMinutes(60);

        public QueryApiStreamLastCalledDate(ContentType contentType, string subverse) : base(new CachePolicy(_timeSpan))
        {
            _contentType = contentType;
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                throw new NotImplementedException("This should never be called");
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.ApiStreamLastCallDate(_contentType, UserName, _subverse);
            }
        }

        public override async Task<DateTime> ExecuteAsync()
        {
            var lastCallDate = await base.ExecuteAsync();
            var newCallDate = Repository.CurrentDate;
            var cacheKey = FullCacheKey;

            CacheHandler.Replace(cacheKey, newCallDate, base.CachingPolicy.Duration);

            EventLogger.Instance.Log(LogType.Debug, "Method Invoked", "QueryApiStreamLastCalledDate", new { lastCallDate, newCallDate, cacheKey, user = User.Identity.Name });

            return lastCallDate;
        }
       
        protected override async Task<DateTime> GetData()
        {
            DateTime last = Repository.CurrentDate;
            last = last.AddMinutes(-5); //first time call or removed from cache, give them a margin
            return last;
        }
    }
}
