using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Voat.Utils
{
    public static class CacheHandler
    {
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();
        private static Dictionary<string, Tuple<Func<object>, TimeSpan>> _meta = new Dictionary<string, Tuple<Func<object>, TimeSpan>>();
        private static Dictionary<string, object> _lockObjects = new Dictionary<string, object>();
        public static object GetData(string key) {
            
            if (String.IsNullOrEmpty(key)) {
                return null;
            }
            key = key.ToLower();

            if (_cache.ContainsKey(key.ToLower()))
            {
                return _cache[key];
            }
            return null;
        }
        
        public static object Register(string key, Func<object> getData, TimeSpan cacheTime, bool reloadUponExpiration = true) {
            if (!String.IsNullOrEmpty(key))
            {
                key = key.ToLower();
                if (!_cache.ContainsKey(key))
                {
                    object o = (_lockObjects.ContainsKey(key) ? _lockObjects[key] : null);
                    if (o == null)
                    {
                        o = new object();
                        _lockObjects[key] = o;
                    }
                    lock (o)
                    {
                        if (!_cache.ContainsKey(key))
                        {
                            var data = getData();
                            _cache[key] = data;
                            _meta[key] = new Tuple<Func<object>, TimeSpan>(getData, cacheTime);
                            if (reloadUponExpiration)
                            {
                                System.Web.HttpContext.Current.Cache.Insert(key, new object(), null, DateTime.Now.Add(cacheTime), System.Web.Caching.Cache.NoSlidingExpiration, new CacheItemUpdateCallback(RefetchItem));
                            }
                            else 
                            {
                                System.Web.HttpContext.Current.Cache.Insert(key, new object(), null, DateTime.Now.Add(cacheTime), System.Web.Caching.Cache.NoSlidingExpiration, new CacheItemUpdateCallback(ExpireItem));
                            }
                            return data;
                        }
                    }
                }
                return _cache[key];
            }
            else 
            {
                throw new ArgumentException("Key can not be null or empty");
            }
        }
        private static void RefetchItem(string key, CacheItemUpdateReason reason, out object value, out CacheDependency dependency, out DateTime exipriation, out TimeSpan slidingExpiration)
        {
            var meta = _meta[key];
            _cache[key] = meta.Item1();

            //assign to keep in cache
            value = new object();
            dependency = null;
            exipriation = DateTime.Now.Add(meta.Item2);
            slidingExpiration = Cache.NoSlidingExpiration;
        }
        private static void ExpireItem(string key, CacheItemUpdateReason reason, out object value, out CacheDependency dependency, out DateTime exipriation, out TimeSpan slidingExpiration)
        {
            _meta.Remove(key);
            _cache.Remove(key);
            value = null;
            dependency = null;
            exipriation = Cache.NoAbsoluteExpiration;
            slidingExpiration = Cache.NoSlidingExpiration;
        }
    }
}