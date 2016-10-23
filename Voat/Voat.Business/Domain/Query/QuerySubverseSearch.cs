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
            using (var repo = new Repository())
            {
                return repo.FindSubverses(_phrase);
            }
        }
    }
}
