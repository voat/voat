using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data;
using Voat.Utilities.Components;

namespace Voat.Caching
{
    public enum CacheType
    {
        Object = 0,
        Dictionary = 1,
        Set = 2
    }

    #region Prototyping
    //public abstract class RefreshEntry
    //{
    //    public TimeSpan CacheTime { get; set; }
    //    public int CurrentCount { get; set; }
    //    public int MaxCount { get; set; }

    //    public T GetData<T>()
    //    {
    //        return (T)ProtectedGetData();
    //    }

    //    protected abstract object ProtectedGetData();
    //}
    //public class TaskRefreshEntry<T> : RefreshEntry
    //{
    //    private Task<T> _task;

    //    public TaskRefreshEntry(Task<T> task)
    //    {
    //        this._task = task;
    //    }

    //    public new async Task<T> GetData()
    //    {
    //        return await _task;
    //    }
    //    protected override object ProtectedGetData()
    //    {
    //        return (object)Task.Run(() => _task);
    //    }
    //}
    #endregion

    public abstract class CacheHandler : ICacheHandler
    {
        private bool _ignoreNulls = true;
        private static ICacheHandler _instance = null;
        private static object _lockInstance = new object();
        private LockStore _lockStore = new LockStore(true);
        private TimeSpan _refreshOffset = TimeSpan.FromSeconds(5);
        private bool _requiresExpirationRemoval = false;
        private bool _cacheEnabled = true;
        //BLOCK: Should be set to true, testing for blocking
        private bool _refetchEnabled = false;

        //holds meta data about the cache item such as the Func, expiration, recacheLimit, and current recaches
        private ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>> _meta = new ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>>();
        
        protected void Initialize()
        {
            //Test lock store for blocking
            //This clears out the LockStore on an interval but I don't see this as the problem. Leaving but commenting out
            //Register("system::lockStore_purge", () =>
            //{
            //    _lockStore.Purge();
            //    return new Object();
            //}, TimeSpan.FromMinutes(30), 0);
        }
       
        protected string StandardizeCacheKey(string cacheKey)
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
            }
        }

        public void Replace<T>(string cacheKey, T replacementValue, TimeSpan? cacheTime = null)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            lock (_lockStore.GetLockObject(cacheKey))
            {
                SetItem(cacheKey, replacementValue, cacheTime);
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

        protected void Remove(string cacheKey, bool isExpiration)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            object o = _lockStore.GetLockObject(cacheKey);
            lock (o)
            {
                if (isExpiration && RequiresExpirationRemoval)
                {
                    DeleteItem(cacheKey);
                }
                else if (!isExpiration)
                {
                    DeleteItem(cacheKey);
                }

                //Attempt removal of metaCache
                Tuple<Func<object>, TimeSpan, int, int> y;
                _meta.TryRemove(cacheKey, out y);
            }
        }
        public void Remove(string cacheKey)
        {
            Remove(cacheKey, false);
        }

        /// <summary>
        /// Registers a function for cache. Locks by key and generates data for return from function
        /// </summary>
        /// <param name="cacheKey">Unique Cache Keys</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="refetchLimit">Value indicating refetch behavior. -1: Do not refetch, 0: Unlimited refetch (use with caution), x > 0: Number of times to refetch cached data</param>
        /// <returns></returns>
        public object Register(string cacheKey, Func<object> getData, TimeSpan cacheTime, int refetchLimit = -1)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            //allow devs to turn off local cache for testing
            if (!CacheEnabled)
            {
                return getData();
            }

            
            if (!Exists(cacheKey))
            {
                ////BLOCK: Temp work around for blocking exploration
                ////Not thread safe
                //bool isRefetchRequest = (RefetchEnabled && cacheTime > TimeSpan.Zero && refetchLimit >= 0);
                //if (!isRefetchRequest)
                //{
                //    var data = getData();
                //    if (data != null || data == null && !_ignoreNulls)
                //    {
                //        SetItem(cacheKey, data, cacheTime);
                //    }
                //    return data;
                //}

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

                                //Refetch Logic
                                if (RefetchEnabled && refetchLimit >= 0)
                                {
                                    _meta[cacheKey] = new Tuple<Func<object>, TimeSpan, int, int>(getData, cacheTime, refetchLimit, 0);
                                    var cache = System.Runtime.Caching.MemoryCache.Default;
                                    var policy = new CacheItemPolicy()
                                    {
                                        AbsoluteExpiration = Repository.CurrentDate.Add(cacheTime.Subtract(_refreshOffset)),
                                        UpdateCallback = new CacheEntryUpdateCallback(RefetchItem)
                                    };
                                    cache.Set(cacheKey, new object(), policy);
                                }
                                else if (RequiresExpirationRemoval)
                                {
                                    var cache = System.Runtime.Caching.MemoryCache.Default;
                                    cache.Add(cacheKey, new object(), new CacheItemPolicy() { AbsoluteExpiration = Repository.CurrentDate.Add(cacheTime), RemovedCallback = new CacheEntryRemovedCallback(ExpireItem) });
                                }
                            }
                            return data;
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.ToString());
                            //Cache now supports Tasks which throw aggregates, if agg has only 1 inner, throw it instead
                            var aggEx = ex as AggregateException;
                            if (aggEx != null && aggEx.InnerExceptions.Count == 1)
                            {
                                throw aggEx.InnerException;
                            }
                            else
                            {
                                throw ex;
                            }
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
                    EventLogger.Log(ex, Domain.Models.Origin.Unknown);
                }
            }
            else
            {
                Debug.Print(String.Format("Expiring cache: {0} - #{1}", cacheKey, refreshCount));
                Remove(cacheKey, true);
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

        public bool Exists(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return ItemExists(cacheKey);
        }

        public void Purge()
        {
            //TODO: Do we need to purge state here?
            //_meta = new ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>>();
            ProtectedPurge();
        }

        #region Properties
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
                                    Debug.Print("CacheHandler.Instance.Contruct({0})", handlerInfo.Type);
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
        public bool CacheEnabled
        {
            get
            {
                return _cacheEnabled;
            }

            set
            {
                _cacheEnabled = value;
            }
        }

        protected bool RequiresExpirationRemoval
        {
            get
            {
                return _requiresExpirationRemoval;
            }

            set
            {
                _requiresExpirationRemoval = value;
            }
        }

        public bool RefetchEnabled
        {
            get
            {
                return _refetchEnabled;
            }

            set
            {
                _refetchEnabled = value;
            }
        }

        #endregion

        #region Abstract Methods

        protected abstract object GetItem(string cacheKey);

        protected abstract void SetItem(string cacheKey, object item, TimeSpan? cacheTime = null);

        protected abstract void DeleteItem(string cacheKey);

        protected abstract bool ItemExists(string cacheKey);

        protected abstract V GetItem<K, V>(string cacheKey, K key, CacheType type);

        protected abstract void SetItem<K, V>(string cacheKey, K key, V item, CacheType type);

        protected abstract void DeleteItem<K>(string cacheKey, K key, CacheType type);

        protected abstract bool ItemExists<K>(string cacheKey, K key, CacheType type);

        protected abstract void ProtectedPurge();
        #endregion

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
            return Convert<T>(val);
        }
        //public Task<T> Register<T>(string cacheKey, Task<T> getData, TimeSpan cacheTime, int recacheLimit = -1)
        //{
        //    var val = Register(cacheKey, new Func<object>(() => { return null; }), cacheTime, recacheLimit);
        //    return Convert<T>(val);
        //}
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
            return (T)Convert<T>(val);
        }

        #endregion Generic Interfaces

        #region Dictionary

        public virtual void DictionaryReplace<K,V>(string cacheKey, K key, Func<V, V> replaceAlg, bool bypassMissing = true)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            lock (_lockStore.GetLockObject(cacheKey))
            {
                V item = (V)GetItem<K,V>(cacheKey, key, CacheType.Dictionary);
                if (!bypassMissing || (bypassMissing && !item.IsDefault())) 
                {
                    SetItem(cacheKey, key, replaceAlg(item), CacheType.Dictionary);
                }
            }
        }

        public virtual void DictionaryRemove<K>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            DeleteItem(cacheKey, key, CacheType.Dictionary);
        }

        public virtual void DictionaryReplace<K,V>(string cacheKey, K key, V newObject)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            SetItem(cacheKey, key, newObject, CacheType.Dictionary);
        }

        public virtual V DictionaryRetrieve<K,V>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return (V)GetItem<K,V>(cacheKey, key, CacheType.Dictionary);
        }

        public virtual bool DictionaryExists<K>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return ItemExists(cacheKey, key, CacheType.Dictionary);
        }

        #endregion

        #region Set

        public void SetRemove<K>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            DeleteItem(cacheKey, key, CacheType.Set);
        }

        public void SetAdd<K>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            SetItem<K, object>(cacheKey, key, null, CacheType.Set);
        }

        public bool SetExists<K>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return ItemExists(cacheKey, key, CacheType.Set);
        }
        #endregion

    }
}
