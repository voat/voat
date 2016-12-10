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
            }), _cacheTime, 5);

            return data.Value;
        }
        //public static void IncrementCount(int submissionID)
        //{
        //    string cacheKey = CachingKey.CommentCount(submissionID);
        //    CacheHandler.Instance.Replace<int?>(cacheKey, x => (x.HasValue ? x + 1 : 1), _cacheTime);
        //}
    }
}
