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

        public QueryApiStreamLastCalledDate(ContentType contentType) : base(new CachePolicy(TimeSpan.FromMinutes(30)))
        {
            _contentType = contentType;
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
                return CachingKey.ApiStreamLastCallDate(_contentType, UserName);
            }
        }

        public override async Task<DateTime> Execute()
        {
            var value = await base.Execute();

            CacheHandler.Instance.Replace<DateTime>(FullCacheKey, Repository.CurrentDate, CachingPolicy.Duration);

            return value;
        }

        protected override DateTime GetData()
        {
            DateTime last = Repository.CurrentDate;
            last = last.AddMinutes(-5); //first time call or removed from cache, give them a margin
            return last;
        }
    }
}
