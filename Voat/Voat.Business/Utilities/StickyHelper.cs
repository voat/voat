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
using Voat.Caching;
using Voat.Data.Models;
using Voat.Domain;
using Voat.Domain.Query;

namespace Voat.Utilities
{
    public static class StickyHelper
    {
        public static Domain.Models.Submission GetSticky(string subverse)
        {

            ////Heads up: Right now the cache is set to ignore nulls, so we create an empty list to use if a sub has no stickies
            ////will refactor this in the future when we modify the cachehandler to support null caching per call
            //List<Submission> stickies = CacheHandler.Instance.Register(CachingKey.StickySubmission(subverse), new Func<List<Submission>>(() =>
            //{
            //    using (var db = new Repository())
            //    {
            //        var x = db.StickiedSubmissions.FirstOrDefault(s => s.Subverse == subverse);
            //        if (x != null)
            //        {
            //            return new List<Submission>() {
            //                DataCache.Submission.Retrieve(x.SubmissionID)
            //            };
            //        }
            //        return new List<Submission>();
            //    }
            //}), TimeSpan.FromSeconds(600));

            var q = new QueryStickies(subverse);
            var stickies = q.Execute();

            if (stickies != null && stickies.Any())
            {
                return stickies.First();
            }
            else
            {
                return null;
            }
        }

        public static void ClearStickyCache(string subverse)
        {
            CacheHandler.Instance.Remove(CachingKey.StickySubmission(subverse));
        }
    }
}
