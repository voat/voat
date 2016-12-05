using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Configuration;
using Voat.Logging;
using Voat.Utilities.Components;

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

        public virtual CachePolicy CachingPolicy
        {
            get;
            protected set; //force policy via constructor
        }

        protected virtual string CacheContainer
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Appends type information to CacheKey. This should not be overriden if possible.
        /// </summary>
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
        public override async Task<T> ExecuteAsync()
        {
            T result = default(T);
            var policy = CachingPolicy;

            if (policy != null && policy.IsValid)
            {
                using (var durationLog = new DurationLogger(EventLogger.Instance, 
                    new LogInformation() {
                        Type = LogType.Debug,
                        Origin = Settings.Origin.ToString(),
                        Category = "Duration",
                        UserName = UserName,
                        Message = $"{this.GetType().Name} ({FullCacheKey})" },
                    TimeSpan.FromSeconds(1)))
                {

                    if (!CacheHandler.Instance.Exists(FullCacheKey))
                    {
                        CacheHit = false;
                        result = await GetData().ConfigureAwait(false);
                        if (!result.IsDefault())
                        {
                            CacheHandler.Instance.Replace(FullCacheKey, result, policy.Duration);
                        }
                    }
                    else
                    {
                        CacheHit = true;
                        result = CacheHandler.Instance.Retrieve<T>(FullCacheKey);
                    }

                    ////Original Code - CacheHandler locks and is preferred 
                    //CacheHit = true;

                    ////BLOCK: This needs fixed
                    //var func = new Func<T>(() => {
                    //    //await GetFreshData();
                    //    var task = Task.Run(GetFreshData);
                    //    Task.WaitAny(task);
                    //    return task.Result;
                    //});

                    //result = CacheHandler.Instance.Register<T>(FullCacheKey.ToLower(), func, CachingPolicy.Duration, CachingPolicy.RefetchLimit);
                }
            }
            else
            {
                CacheHit = true;
                result = await GetFreshData().ConfigureAwait(false);
            }
            return result;
        }
        
        /// <summary>
        /// Retreives fresh data - this should be the only place new data is retreived in derived classes
        /// </summary>
        /// <returns></returns>
        protected abstract Task<T> GetData();

        //BLOCK: This needs fixed
        private async Task<T> GetFreshData()
        {
            //BLOCK: This needs fixed
            CacheHit = false;
            Debug.Print("{0}(loading)", this.GetType().Name);
            T data = await GetData().ConfigureAwait(false);
            return data;
        }
    }
}
