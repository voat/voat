using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static object GetData(string key) 
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
                if (_cache.ContainsKey(key))
                {
                    _cache.Remove(key);
                }
                if (_meta.ContainsKey(key))
                {
                    _meta.Remove(key);
                }
                if (_lockObjects.ContainsKey(key))
                {
                    _lockObjects.Remove(key);
                }
                //System.Web.HttpContext.Current.Cache.Remove(key);
            }

        }
        public static object Register(string key, Func<object> getData, TimeSpan cacheTime, bool reloadUponExpiration = true) 
        {
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
                            try
                            {
                                Debug.Print("Inserting Cache: " + key);
                                var data = getData();
                                _cache[key] = data;
                                _meta[key] = new Tuple<Func<object>, TimeSpan>(getData, cacheTime);
                                if (reloadUponExpiration)
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
                throw new ArgumentException("Key can not be null or empty");
            }
        }
        private static void RefetchItem(string key, CacheItemUpdateReason reason, out object value, out CacheDependency dependency, out DateTime exipriation, out TimeSpan slidingExpiration)
        {

            Debug.Print("Refreshing cache: " + key);
            var meta = _meta[key];
            //assign to keep in cache
            value = new object();
            dependency = null;
            exipriation = DateTime.Now.Add(meta.Item2);
            slidingExpiration = Cache.NoSlidingExpiration; 
            try
            {
                var data = meta.Item1();
                _cache[key] = data;
            }
            catch (Exception ex) 
            {
                Debug.Print(ex.ToString());
                /*no-op*/
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
        private static void ExpireItem(string key, CacheItemUpdateReason reason, out object value, out CacheDependency dependency, out DateTime exipriation, out TimeSpan slidingExpiration)
        {
             value = null;
            dependency = null;
            exipriation = Cache.NoAbsoluteExpiration;
            slidingExpiration = Cache.NoSlidingExpiration;

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
    }
}