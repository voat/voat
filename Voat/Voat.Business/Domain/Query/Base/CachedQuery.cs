using System;
using System.Threading.Tasks;

using Voat.Caching;

namespace Voat.Domain.Query
{
    public abstract class CachedQuery<T> : Query<T>, ICacheable
    {
        private bool _cacheHit = false;

        public CachedQuery(CachePolicy policy)
        {
            CachingPolicy = (policy != null ? policy : CachePolicy.None); //Ensure we have a cache policy
        }

        /// <summary>
        /// After the Execute() method returns this value will signify if the Cache was hit or if data was pulled fresh.
        /// </summary>
        public bool CacheHit
        {
            get
            {
                return _cacheHit;
            }
            private set
            {
                _cacheHit = value;
            }
        }

        /// <summary>
        /// Implements a unique key. Override this method and provide a unique value.
        /// </summary>
        public abstract string CacheKey { get; }

        public CachePolicy CachingPolicy
        {
            get;
            private set; //force policy via constructor
        }

        //{
        //    get { throw new InvalidOperationException("Derived classes must override either CacheKey or FullCacheKey properties"); }
        //}

        protected virtual string CacheContainer
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Appends type information to CacheKey. This should not be overriden if possible.
        /// </summary>
        /// <param name="useHashedKey"></param>
        /// <returns></returns>
        protected virtual string FullCacheKey
        {
            get
            {
                string key = CacheKey;

                var full = String.Format("{0}:{1}", CacheContainer, (String.IsNullOrWhiteSpace(key) ? "default" : key));

                return full;
            }
        }

        public override T Execute()
        {
            if (CachingPolicy != null && CachingPolicy.IsValid)
            {
                //I think this will keep data in memory. Need to have method that just inserts data.
                CacheHit = true;
                return CacheHandler.Instance.Register<T>(FullCacheKey.ToLower(), GetFreshData, CachingPolicy.Duration, CachingPolicy.RecacheLimit);
            }
            else
            {
                CacheHit = true;
                return GetFreshData();
            }
        }

        /// <summary>
        /// Retreives fresh data - this should be the only place new data is retreived in derived classes
        /// </summary>
        /// <returns></returns>
        protected abstract T GetData();

        private T GetFreshData()
        {
            CacheHit = false;
            T data = GetData();
            return data;
        }
    }
}
