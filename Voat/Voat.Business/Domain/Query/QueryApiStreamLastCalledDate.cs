using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

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

        public override DateTime Execute()
        {
            var value = base.Execute();

            CacheHandler.Instance.Replace<DateTime>(FullCacheKey, Repository.CurrentDate, CachingPolicy.Duration);

            return value;
        }

        protected override async Task<DateTime> GetData()
        {
            DateTime last = Repository.CurrentDate;
            last = last.AddMinutes(-5); //first time call or removed from cache, give them a margin
            return last;
        }
    }
}
