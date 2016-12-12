using System;
using System.Threading.Tasks;

namespace Voat.Caching
{
    public interface ICacheHandler
    {
        /// <summary>
        /// Determines if cache is enabled
        /// </summary>
        bool CacheEnabled { get; set; }

        /// <summary>
        /// Determines if cache will honor refetch parameters and background refetch entries if specified
        /// </summary>
        bool RefetchEnabled { get; set; }

        /// <summary>
        /// Checks if a cached object exists at specified key
        /// </summary>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <returns></returns>
        bool Exists(string cacheKey);

        ///// <summary>
        ///// If cached item was registered using the Register function, will execute the Func and refresh data at specified key
        ///// </summary>
        ///// <param name="cacheKey">Unique Cache Key</param>
        ///// <returns></returns>
        //Task Refresh(string cacheKey);

        /// <summary>
        /// Registers a cache Func with caching runtime. Will return the cached data if it exists, if it doesn't will execute func and store in cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="refetchLimit">Value indicating refresh behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        T Register<T>(string cacheKey, Func<T> getData, TimeSpan cacheTime, int refetchLimit = -1);

        /// <summary>
        /// Registers a cache Func with caching runtime. Will return the cached data if it exists, if it doesn't will execute func and store in cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="refetchLimit">Value indicating refresh behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        Task<T> Register<T>(string cacheKey, Func<Task<T>> getData, TimeSpan cacheTime, int refetchLimit = -1);
        
        /// <summary>
        /// Removes cached item at key
        /// </summary>
        /// <param name="cacheKey">Unique Cache Key</param>
        void Remove(string cacheKey);

        /// <summary>
        /// Purges all cache items
        /// </summary>
        void Purge();

        /// <summary>
        /// Replaces cached item at key after processing via Func.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <param name="replaceAlg"></param>
        void Replace<T>(string cacheKey, Func<T, T> replaceAlg, TimeSpan? cacheTime = null);

        /// <summary>
        /// Replaces cached item at key after processing via Func
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <param name="replaceAlg"></param>
        void Replace<T>(string cacheKey, T replacementValue, TimeSpan? cacheTime = null);

        /// <summary>
        /// Replaces cached item at key after processing via Func.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <param name="replaceAlg"></param>
        void ReplaceIfExists<T>(string cacheKey, Func<T, T> replaceAlg, TimeSpan? cacheTime = null);

        /// <summary>
        /// Replaces cached item at key after processing via Func
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <param name="replaceAlg"></param>
        void ReplaceIfExists<T>(string cacheKey, T replacementValue, TimeSpan? cacheTime = null);

        /// <summary>
        /// Returns object in cache with specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <returns></returns>
        T Retrieve<T>(string cacheKey);

        #region Dictionary Based

        /// <summary>
        /// Removes dictionary item in cached dictionary at specified key
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="key"></param>
        void DictionaryRemove<K>(string cacheKey, K key);

        /// <summary>
        /// Method will insert or replace cache object at specified dictionary key.
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <typeparam name="V">Value Type</typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="key"></param>
        /// <param name="newObject"></param>
        void DictionaryReplace<K,V>(string cacheKey, K key, V newObject);

        /// <summary>
        /// Replaces dictionary item at key after processing via Func
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <typeparam name="V">Value Type</typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="key"></param>
        /// <param name="replaceAlg"></param>
        void DictionaryReplace<K,V>(string cacheKey, K key, Func<V, V> replaceAlg, bool bypassMissing = true);

        /// <summary>
        /// Retrieves dictionary item in cached dictionary at specified key
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <typeparam name="V">Value Type</typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        V DictionaryRetrieve<K,V>(string cacheKey, K key);

        /// <summary>
        /// Checks if a cached dictionary contains specified key
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool DictionaryExists<K>(string cacheKey, K key);

        #endregion DictionaryBased

        #region Set Based

        void SetRemove<T>(string cacheKey, T key);

        void SetAdd<T>(string cacheKey, T key);

        bool SetExists<T>(string cacheKey, T key);

        #endregion SetBased
    }
}
