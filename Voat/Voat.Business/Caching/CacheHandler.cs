using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Voat.Utilities.Components;
using Voat.Common;
using Voat.Data;

namespace Voat.Caching
{
    public abstract class CacheHandler : ICacheHandler
    {
        private bool _ignoreNulls = true;
        private static ICacheHandler _instance = null;
        private static object _lockInstance = new object();
        private LockStore _lockStore = new LockStore();

        //holds meta data about the cache item such as the Func, expiration, recacheLimit, and current recaches
        private ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>> _meta = new ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>>();

        public static ICacheHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockInstance)
                    {
                        if (_instance == null)
                        {
                            try
                            {
                                var handlerInfo = CacheHandlerSection.Instance.Handler;
                                if (handlerInfo != null)
                                {
                                    _instance = handlerInfo.Construct();
                                }
                            }
                            catch (Exception ex)
                            {
                                EventLogger.Log(ex);
                            }
                            finally
                            {
                                if (_instance == null)
                                {
                                    //Can't load type, default to no caching.
                                    _instance = new NullCacheHandler();
                                }
                            }
                        }
                    }
                }
                return _instance;
            }

            set
            {
                _instance = value;
            }
        }

        protected abstract object GetItem(string cacheKey);

        protected abstract void SetItem(string cacheKey, object item, TimeSpan? cacheTime = null);

        protected abstract void DeleteItem(string cacheKey);

        protected abstract bool ItemExists(string cacheKey);

        protected abstract object GetItem(string cacheKey, object dictionaryKey);

        protected abstract void SetItem(string cacheKey, object dictionaryKey, object item);

        protected abstract void DeleteItem(string cacheKey, object dictionaryKey);

        protected abstract bool ItemExists(string cacheKey, object dictionaryKey);

        private bool cacheEnabled = true;

        public bool CacheEnabled
        {
            get
            {
                return cacheEnabled;
            }

            set
            {
                cacheEnabled = value;
            }
        }

        private string StandardizeCacheKey(string cacheKey)
        {
            if (String.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentException("CacheKey can not be null or empty.", cacheKey);
            }
            return cacheKey.ToLower();
        }

        public void Replace<T>(string cacheKey, Func<T, T> replaceAlg, TimeSpan? cacheTime = null)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            lock (_lockStore.GetLockObject(cacheKey))
            {
                SetItem(cacheKey, replaceAlg(Retrieve<T>(cacheKey)), cacheTime);

                //_cache[key] = replaceAlg(Retrieve<T>(key));
            }
        }

        public void Replace<T>(string cacheKey, object dictionaryKey, Func<T, T> replaceAlg)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            lock (_lockStore.GetLockObject(cacheKey))
            {
                T item = (T)GetItem(cacheKey, dictionaryKey);
                SetItem(cacheKey, dictionaryKey, replaceAlg(item));

                //_cache[key] = replaceAlg(Retrieve<T>(key));
            }
        }

        public void Replace<T>(string cacheKey, T replacementValue, TimeSpan? cacheTime = null)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            lock (_lockStore.GetLockObject(cacheKey))
            {
                SetItem(cacheKey, replacementValue, cacheTime);

                //_cache[key] = replaceAlg(Retrieve<T>(key));
            }
        }

        public object Retrieve(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            if (Exists(cacheKey))
            {
                return GetItem(cacheKey);
            }
            return null;
        }

        public void Remove(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            object o = _lockStore.GetLockObject(cacheKey);
            lock (o)
            {
                DeleteItem(cacheKey);
                Tuple<Func<object>, TimeSpan, int, int> y;
                _meta.TryRemove(cacheKey, out y);
            }
        }

        /// <summary>
        /// Registers a function for cache. Locks by key and generates data for return from function
        /// </summary>
        /// <param name="cacheKey">Unique Cache Keys</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="recacheLimit">Value indicating refresh behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        public object Register(string cacheKey, Func<object> getData, TimeSpan cacheTime, int recacheLimit = -1)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            //allow devs to turn off local cache for testing
            if (!CacheEnabled)
            {
                return getData();
            }

            cacheKey = cacheKey.ToLower();
            if (!Exists(cacheKey))
            {
                object o = _lockStore.GetLockObject(cacheKey);
                lock (o)
                {
                    if (!Exists(cacheKey))
                    {
                        try
                        {
                            var data = getData();
                            if (data != null || data == null && !_ignoreNulls)
                            {
                                Debug.Print("Inserting Cache: " + cacheKey);
                                SetItem(cacheKey, data, cacheTime);
                                _meta[cacheKey] = new Tuple<Func<object>, TimeSpan, int, int>(getData, cacheTime, recacheLimit, 0);
                                if (recacheLimit >= 0)
                                {
                                    //old code is http context dependent, rewriting to use cache directly
                                    var cache = System.Runtime.Caching.MemoryCache.Default;
                                    var policy = new CacheItemPolicy()
                                    {
                                        //SlidingExpiration = TimeSpan.Zero,
                                        AbsoluteExpiration = Repository.CurrentDate.Add(cacheTime),
                                        UpdateCallback = new CacheEntryUpdateCallback(RefetchItem)
                                    };

                                    cache.Set(cacheKey, new object(), policy);
                                }
                                else
                                {
                                    //old code is http context dependent, rewriting to use cache directly
                                    var cache = System.Runtime.Caching.MemoryCache.Default;
                                    cache.Add(cacheKey, new object(), new CacheItemPolicy() { AbsoluteExpiration = Repository.CurrentDate.Add(cacheTime), RemovedCallback = new CacheEntryRemovedCallback(ExpireItem) });
                                }
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
            var cacheItem = GetItem(cacheKey);
            return cacheItem;
        }

        public Task Refresh(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            var task = new Task(() =>
            {
                if (Exists(cacheKey))
                {
                    object o = _lockStore.GetLockObject(cacheKey);
                    lock (o)
                    {
                        var meta = _meta[cacheKey];
                        var data = meta.Item1(); //get new data
                        SetItem(cacheKey, data, meta.Item2);
                    }
                }
            });
            task.Start();
            return task;
        }

        private void RefetchItem(CacheEntryUpdateArguments args)
        {
            var cacheKey = args.Key;
            var meta = _meta[cacheKey];
            int refreshLimit = meta.Item3;
            int refreshCount = meta.Item4 + 1;

            if (refreshLimit == 0 || refreshCount <= refreshLimit)
            {
                Debug.Print(String.Format("Refreshing cache: {0} - #{1}", cacheKey, refreshCount));
                _meta[cacheKey] = new Tuple<Func<object>, TimeSpan, int, int>(meta.Item1, meta.Item2, meta.Item3, refreshCount);

                args.UpdatedCacheItem = new CacheItem(cacheKey, new object());

                args.UpdatedCacheItemPolicy = new CacheItemPolicy()
                {
                    //SlidingExpiration = TimeSpan.Zero,
                    AbsoluteExpiration = Repository.CurrentDate.Add(meta.Item2),
                    UpdateCallback = new CacheEntryUpdateCallback(RefetchItem)
                };

                try
                {
                    var data = meta.Item1(); //get new data
                    SetItem(cacheKey, data, meta.Item2);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
            else
            {
                Debug.Print(String.Format("Expiring cache: {0} - #{1}", cacheKey, refreshCount));
                Remove(cacheKey);
            }
        }

        private void ExpireItem(CacheEntryRemovedArguments args)
        {
            try
            {
                Debug.Print(args.RemovedReason.ToString());
                Debug.Print(String.Format("Expiring cache: {0}", args.CacheItem.Key));
                if (args.RemovedReason != CacheEntryRemovedReason.CacheSpecificEviction)
                {
                    Remove(args.CacheItem.Key);
                }
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
        public T Register<T>(string cacheKey, Func<T> getData, TimeSpan cacheTime, int recacheLimit = -1)
        {
            var val = Register(cacheKey, new Func<object>(() => { return getData(); }), cacheTime, recacheLimit);
            return (T)Convert<T>(val);
        }

        private T Convert<T>(object val)
        {
            if (val == null)
            {
                return (T)val;
            }

            //if type can be cast, we cast it, else we convert it.
            if (typeof(T).IsAssignableFrom(val.GetType()))
            {
                return (T)val;
            }
            else
            {
                //Everything stored in redis returns as IDicationary<object, object>. This converts it to the appropriate type
                if (val is IDictionary<object, object>)
                {
                    Type genericType = typeof(T);
                    object dictionary = null;
                    if (genericType.IsGenericType && typeof(T).GetGenericTypeDefinition().IsAssignableFrom(typeof(IDictionary<,>)))
                    {
                        //we have a specific dictionary
                        Type[] genericArgs = genericType.GetGenericArguments();
                        Type dictionaryType = ((typeof(Dictionary<,>).MakeGenericType(genericArgs)));
                        dictionary = Activator.CreateInstance(dictionaryType);
                        IDictionary<object, object> existing = ((IDictionary<object, object>)val);
                        foreach (KeyValuePair<object, object> kp in existing)
                        {
                            object key = null;
                            Type keyType = genericArgs[0];

                            //Enum based keys broke Redis Cache - trap and parse them
                            if (keyType.IsEnum)
                            {
                                key = Enum.Parse(keyType, kp.Key.ToString());
                            }
                            else
                            {
                                key = System.Convert.ChangeType(kp.Key, keyType);
                            }
                            ((dynamic)dictionary).Add((dynamic)key, (dynamic)kp.Value);
                        }
                    }
                    return (T)dictionary;
                }
                else
                {
                    if (val != null)
                    {
                        //HACK: Need a better way to handle redis to .NET type mapping. We store int? in cache on UI and Redis conversions out of cache throw invalid cast exceptions.
                        Type castType = typeof(T);
                        if (castType.IsGenericType)
                        {
                            //check for Nullable<>
                            if (castType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                //redis stores numbers in Int64
                                if (val.GetType() == typeof(long))
                                {
                                    if (castType.GetGenericArguments()[0] == typeof(int))
                                    {
                                        //this is just the one exception we are using
                                        return (T)castType.GetConstructor(castType.GetGenericArguments()).Invoke(new object[] { System.Convert.ChangeType(val, typeof(int)) });
                                    }
                                }

                                return (T)castType.GetConstructor(castType.GetGenericArguments()).Invoke(new object[] { val });
                            }
                        }
                        else if (castType.IsPrimitive)
                        {
                            return (T)System.Convert.ChangeType(val, castType);
                        }
                    }

                    //default direct cast
                    return (T)val;
                }
            }
        }

        public T Retrieve<T>(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            var val = Retrieve(cacheKey);

            //if (val is IConvertible) {
            //    return (T)((IConvertible)val).ToType(typeof(T), FormatProvider );
            //}
            return (T)Convert<T>(val);
        }

        public bool Exists(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return ItemExists(cacheKey);
        }

        public void Remove(string cacheKey, object dictionaryKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            DeleteItem(cacheKey, dictionaryKey);
        }

        public void Replace<T>(string cacheKey, object dictionaryKey, T newObject)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            SetItem(cacheKey, dictionaryKey, newObject);
        }

        public T Retrieve<T>(string cacheKey, object dictionaryKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return (T)GetItem(cacheKey, dictionaryKey);
        }

        public bool Exists(string cacheKey, object dictionaryKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return ItemExists(cacheKey, dictionaryKey);
        }

        protected abstract void ProtectedPurge();

        public void Purge()
        {
            //TODO: Do we need to purge state here?
            //_meta = new ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>>();
            ProtectedPurge();
        }

        #endregion Generic Interfaces
    }
}
