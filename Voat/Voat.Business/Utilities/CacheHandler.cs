using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Caching;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Voat.Utilities
{
    public static class CacheHandler
    {

        public static class Keys {

            public static string CommentTree(int submissionID)
            {
                return String.Format("Comment.Tree.{0}", submissionID).ToLower();
            }
            public static string Submission(int submissionID)
            {
                return String.Format("submission.{0}", submissionID).ToLower();
            }
            public static string SubverseInfo(string subverse) {
                return String.Format("subverse.{0}.info", subverse).ToLower();
            }
            public static string Search(string subverse, string query) {
                return String.Format("search.{0}.{1}", subverse, query).ToLower();
            }
            public static string Search(string query) {
                return Search("all", query);
            }

        }

        private static bool cacheEnabled = true;

        public static bool CacheEnabled 
        {
            get {
                return cacheEnabled;
            }
            set {
                cacheEnabled = value;
            }
        }

        //data kept in dictionary to serve as hot cache
        private static ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        //holds meta data about the cache item such as the Func, expiration, recacheLimit, and current recaches
        private static ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>> _meta = new ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>>();
        
        //serves as a holder for lock object to syncronize the cache handler
        private static Dictionary<string, object> _lockObjects = new Dictionary<string, object>();

        private static object sLock = new object();

        public static void Replace<T>(string key, Func<T,T> replaceAlg)
        {
            if (!String.IsNullOrEmpty(key))
            {
                key = key.ToLower();

                lock (GetLockObject(key))
                {
                    _cache[key] = replaceAlg(Retrieve<T>(key));
                }
            }
        }
        public static object Retrieve(string key) 
        {
            if (!String.IsNullOrEmpty(key)) 
            {
                key = key.ToLower();

                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }
            }
            return null;
        }
        public static void Remove(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {

                key = key.ToLower();

                object o = GetLockObject(key);
                lock (o)
                {
                    object x;
                    _cache.TryRemove(key, out x);
                    Tuple<Func<object>, TimeSpan, int, int> y;
                    _meta.TryRemove(key, out y);
                }
            }
        }
        internal static object GetLockObject(string key) 
        {
            lock (sLock) {
                
                object o = (_lockObjects.ContainsKey(key) ? _lockObjects[key] : null);
                
                if (o == null) {
                    o = new object();
                    _lockObjects[key] = o;
                }
                
                return o;
            }
        }
        
       
        /// <summary>
        /// Registers a function for cache. Locks by key and generates data for return from function
        /// </summary>
        /// <param name="key">Unique Cache Keys</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="recacheLimit">Value indicating refresh behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        public static object Register(string key, Func<object> getData, TimeSpan cacheTime, int recacheLimit = -1) 
        {
            if (!String.IsNullOrEmpty(key))
            {
                //allow devs to turn off local cache for testing
                if (!CacheEnabled) {
                    return getData();
                }

                key = key.ToLower();
                if (!_cache.ContainsKey(key))
                {
                    object o = GetLockObject(key);
                    lock (o)
                    {
                        if (!_cache.ContainsKey(key))
                        {
                            try
                            {
                                Debug.Print("Inserting Cache: " + key);
                                var data = getData();
                                _cache[key] = data;
                                _meta[key] = new Tuple<Func<object>, TimeSpan, int, int>(getData, cacheTime, recacheLimit, 0);
                                if (recacheLimit >= 0)
                                {
                                    System.Web.HttpContext.Current.Cache.Insert(key, new object(), null, DateTime.Now.Add(cacheTime), System.Web.Caching.Cache.NoSlidingExpiration, new CacheItemUpdateCallback(RefetchItem));
                                }
                                else
                                {
                                    System.Web.HttpContext.Current.Cache.Insert(key, new object(), null, DateTime.Now.Add(cacheTime), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, new CacheItemRemovedCallback(ExpireItem));
                                }
                                return data;
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.ToString());
                                throw ex;
                            }
                        }
                    }
                }
                return _cache[key];
            }
            else 
            {
                throw new ArgumentException("Keys can not be null or empty");
            }
        }
        public static Task Refresh(string key) {

            var task = new Task(() => {
                if (_cache.ContainsKey(key))
                {
                    object o = GetLockObject(key);
                    lock (o)
                    {
                        var meta = _meta[key];
                        var data = meta.Item1(); //get new data
                        _cache[key] = data;
                    }
                }
            });
            task.Start();
            return task;

        }
        private static void RefetchItem(string key, CacheItemUpdateReason reason, out object value, out CacheDependency dependency, out DateTime exipriation, out TimeSpan slidingExpiration)
        {
            var meta = _meta[key];
            int refreshLimit = meta.Item3;
            int refreshCount = meta.Item4 + 1;

            if (refreshLimit == 0 || refreshCount <= refreshLimit)
            {
                Debug.Print(String.Format("Refreshing cache: {0} - #{1}", key, refreshCount));
                _meta[key] = new Tuple<Func<object>, TimeSpan, int, int>(meta.Item1, meta.Item2, meta.Item3, refreshCount);
                
                //reset into cache dummy object to get callback
                value = new object();
                dependency = null;
                exipriation = DateTime.Now.Add(meta.Item2);
                slidingExpiration = Cache.NoSlidingExpiration;
                
                try
                {
                    var data = meta.Item1(); //get new data
                    _cache[key] = data;
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                    /*no-op*/
                }
            }
            else 
            {
                Debug.Print(String.Format("Expiring cache: {0} - #{1}", key, refreshCount));

                //expire cache
                value = null;
                dependency = null;
                exipriation = Cache.NoAbsoluteExpiration;
                slidingExpiration = Cache.NoSlidingExpiration;
                Remove(key);
            }
        }
        private static void ExpireItem(string key, object value, CacheItemRemovedReason reason)
        {
            try
            {
                Debug.Print(String.Format("Expiring cache: {0}", key));
                Remove(key);

            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        #region Generic Interfaces
        /// <summary>
        /// Registers a function for cache. Locks by key and generates data for return from function
        /// </summary>
        /// <param name="key">Unique Cache Keys</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="recacheLimit">Value indicating refresh behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        public static T Register<T>(string key, Func<T> getData, TimeSpan cacheTime, int recacheLimit = -1)
        {
            return (T)Register(key, new Func<object>(() => { return getData(); }), cacheTime, recacheLimit);
        }
        public static T Retrieve<T>(string key)
        {
            return (T)Retrieve(key);
        }
        
        #endregion 
    }
}