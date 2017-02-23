using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QuerySet : CachedQuery<Voat.Domain.Models.Set>
    {
        private string _name;
        private string _ownerName;


        public QuerySet(string name, string ownerName) : base(new CachePolicy(TimeSpan.FromMinutes(30)))
        {
            _name = name;
            _ownerName = ownerName;
        }

        public override string CacheKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.Set(_name, _ownerName);
            }
        }

        protected override async Task<Voat.Domain.Models.Set> GetData()
        {
            using (var db = new Repository())
            {
                var subverseSet = db.GetSet(_name, _ownerName);
                var set = subverseSet.Map();
                return set;
            }
        }
    }
}
