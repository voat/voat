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
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class CommentCounter
    {
        private static TimeSpan _cacheTime = TimeSpan.FromMinutes(4);

        public static int CommentCount(int submissionID)
        {
            string cacheKey = CachingKey.CommentCount(submissionID);
            var data = CacheHandler.Instance.Register(cacheKey, new Func<int?>(() =>
            {
                using (var repo = new Repository())
                {
                    return repo.GetCommentCount(submissionID);
                }
            }), _cacheTime, 3);

            return data.Value;
        }
        //public static void IncrementCount(int submissionID)
        //{
        //    string cacheKey = CachingKey.CommentCount(submissionID);
        //    CacheHandler.Instance.Replace<int?>(cacheKey, x => (x.HasValue ? x + 1 : 1), _cacheTime);
        //}
    }
}
