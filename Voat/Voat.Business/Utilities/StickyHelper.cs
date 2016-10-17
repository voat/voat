using System;
using System.Collections.Generic;
using System.Linq;
using Voat.Caching;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class StickyHelper
    {
        public static Submission GetSticky(string subverse)
        {
            //Heads up: Right now the cache is set to ignore nulls, so we create an empty list to use if a sub has no stickies
            //will refactor this in the future when we modify the cachehandler to support null caching per call
            List<Submission> stickies = CacheHandler.Instance.Register(CachingKey.StickySubmission(subverse), new Func<List<Submission>>(() =>
            {
                using (var db = new voatEntities())
                {
                    var x = db.StickiedSubmissions.FirstOrDefault(s => s.Subverse == subverse);
                    if (x != null)
                    {
                        return new List<Submission>() { DataCache.Submission.Retrieve(x.SubmissionID) };
                    }
                    return new List<Submission>();
                }
            }), TimeSpan.FromSeconds(600));

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
