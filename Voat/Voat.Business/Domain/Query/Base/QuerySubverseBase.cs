using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    /// <summary>
    /// This is a base type to abstract the base subverse query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QuerySubverseBase<T> : CachedQuery<T>
    {
        protected string _subverse;
        protected SearchOptions _options;

        public QuerySubverseBase(string subverse, SearchOptions options) : base(new CachePolicy(TimeSpan.FromMinutes(5)))
        {
            this._subverse = subverse;
            this._options = options;
        }

        public override string CacheKey
        {
            get
            {
                throw new NotImplementedException("Override FullCacheKey in derived classes");
            }
        }
    }
}
