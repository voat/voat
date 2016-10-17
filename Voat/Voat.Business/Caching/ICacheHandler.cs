using System;
using System.Threading.Tasks;

namespace Voat.Caching
{
    //Might seperate this interface into object hanlding and dictionary handling.
    public interface ICacheHandler
    {
        bool CacheEnabled { get; set; }

        /// <summary>
        /// Checks if a cached object exists at specified key
        /// </summary>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <returns></returns>
        bool Exists(string cacheKey);

        /// <summary>
        /// If cached item was registered using the Register function, will execute the Func and refresh data at specified key
        /// </summary>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <returns></returns>
        Task Refresh(string cacheKey);

        /// <summary>
        /// Registers a cache Func with caching runtime. Will return the cached data if it exists, if it doesn't will execute func and store in cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="recacheLimit">Value indicating refresh behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        T Register<T>(string cacheKey, Func<T> getData, TimeSpan cacheTime, int recacheLimit = -1);

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
        /// Returns object in cache with specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Unique Cache Key</param>
        /// <returns></returns>
        T Retrieve<T>(string cacheKey);

        #region DictionaryBased

        //T RegisterDictonary<T>(string cacheKey, Func<T> getData, TimeSpan cacheTime, int recacheLimit = -1) where T : IDictionary;
        //IEnumerable<T> RegisterDictionary<T>(string cacheKey, Func<IEnumerable<T>> getData, Func<T, object> getDictionaryKey, TimeSpan cacheTime, int recacheLimit = -1);

        /// <summary>
        /// Removes dictionary item in cached dictionary at specified key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="dictionaryKey"></param>
        void Remove(string cacheKey, object dictionaryKey);

        /// <summary>
        /// Method will insert or replace cache object at specified dictionary key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="dictionaryKey"></param>
        /// <param name="newObject"></param>
        void Replace<T>(string cacheKey, object dictionaryKey, T newObject);

        /// <summary>
        /// Replaces dictionary item at key after processing via Func
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="dictionaryKey"></param>
        /// <param name="replaceAlg"></param>
        void Replace<T>(string cacheKey, object dictionaryKey, Func<T, T> replaceAlg);

        /// <summary>
        /// Retrieves dictionary item in cached dictionary at specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="dictionaryKey"></param>
        /// <returns></returns>
        T Retrieve<T>(string cacheKey, object dictionaryKey);

        /// <summary>
        /// Checks if a cached dictionary contains specified key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="dictionaryKey"></param>
        /// <returns></returns>
        bool Exists(string cacheKey, object dictionaryKey);

        #endregion DictionaryBased
    }
}
