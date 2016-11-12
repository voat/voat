using System;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class CommentCounter
    {
        public static int CommentCount(int submissionID)
        {
            string cacheKey = CachingKey.CommentCount(submissionID);
            var data = CacheHandler.Instance.Register(cacheKey, new Func<int?>(() =>
            {
                using (var repo = new Repository())
                {
                    return repo.GetCommentCount(submissionID);
                }
            }), TimeSpan.FromMinutes(4), 1);

            return data.Value;
        }
    }
}
