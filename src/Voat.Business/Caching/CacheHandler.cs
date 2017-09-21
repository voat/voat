#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Logging;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat.Caching
{
    //Because MS loves to keep us on our toes. MemoryCache is not self monitoring
    public class MonitoredMemoryCache : MemoryCache
    {
        private Timer _timer;

        public MonitoredMemoryCache(IOptions<MemoryCacheOptions> options) : base(options)
        {
            _timer = new Timer(new TimerCallback(x => {
                this.Compact(0);
            }), this, options.Value.ExpirationScanFrequency, options.Value.ExpirationScanFrequency);
        }
        protected override void Dispose(bool disposing)
        {
            _timer.Dispose();
            base.Dispose(disposing);
        }
    }

    public enum CacheType
    {
        Object = 0,
        Dictionary = 1,
        Set = 2,
        List = 3
    }

    #region Prototyping
    public abstract class RefetchEntry
    {
        public TimeSpan CacheTime { get; set; }
        public int CurrentCount { get; set; }
        public int MaxCount { get; set; }

    }
    public class RefetchEntryTask<T> : RefetchEntry
    {
        private Func<Task<T>> _task;

        public RefetchEntryTask(Func<Task<T>> task)
        {
            this._task = task;
        }

        public T GetData()
        {
            return Task.Run(_task).Result;
        }
    }
    public class RefetchEntryFunc<T> : RefetchEntry
    {
        private Func<T> _func;

        public RefetchEntryFunc(Func<T> func)
        {
            _func = func;
        }
        public T GetData()
        {
            return _func();
        }
    }
    #endregion

    public abstract class CacheHandler : ICacheHandler
    {
        private bool _ignoreNulls = true;
        private static ICacheHandler _instance = null;
        private static object _lockInstance = new object();
        private SemaphoreSlimLockStore _semaphoreSlimLockStore = new SemaphoreSlimLockStore(true);
        private TimeSpan _refreshOffset = TimeSpan.FromSeconds(5);
        private bool _requiresExpirationRemoval = false;
        private bool _cacheEnabled = true;
        private bool _refetchEnabled = true;
        private IMemoryCache _expirationTrackingCache;

        //holds meta data about the cache item such as the Func, expiration, recacheLimit, and current recaches
        //private ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>> _meta = new ConcurrentDictionary<string, Tuple<Func<object>, TimeSpan, int, int>>();
        private ConcurrentDictionary<string, RefetchEntry> _meta = new ConcurrentDictionary<string, RefetchEntry>();

        public CacheHandler(bool refetchEnabled = true)
        {
            _refetchEnabled = refetchEnabled;
        }

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

            var sema = _semaphoreSlimLockStore.GetLockObject(cacheKey);
            sema.Wait();
            try
            {
                SetItem(cacheKey, replaceAlg(Retrieve<T>(cacheKey)), cacheTime);
            }
            finally
            {
                sema.Release();
            }

            //lock (_lockStore.GetLockObject(cacheKey))
            //{
            //    SetItem(cacheKey, replaceAlg(Retrieve<T>(cacheKey)), cacheTime);
            //}
        }

        public void Replace<T>(string cacheKey, T replacementValue, TimeSpan? cacheTime = null)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            var sema = _semaphoreSlimLockStore.GetLockObject(cacheKey);
            sema.Wait();
            try
            {
                SetItem(cacheKey, replacementValue, cacheTime);
            }
            finally
            {
                sema.Release();
            }

            //lock (_lockStore.GetLockObject(cacheKey))
            //{
            //    SetItem(cacheKey, replacementValue, cacheTime);
            //}
        }
        public void ReplaceIfExists<T>(string cacheKey, Func<T, T> replaceAlg, TimeSpan? cacheTime = default(TimeSpan?))
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            if (Exists(cacheKey))
            {
                Replace(cacheKey, replaceAlg, cacheTime);
            }
        }

        public void ReplaceIfExists<T>(string cacheKey, T replacementValue, TimeSpan? cacheTime = default(TimeSpan?))
        {
            if (Exists(cacheKey))
            {
                Replace(cacheKey, replacementValue, cacheTime);
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

            var sema = _semaphoreSlimLockStore.GetLockObject(cacheKey);
            sema.Wait();
            try
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
                RefetchEntry y;
                _meta.TryRemove(cacheKey, out y);

                //Remove Expiration Tracking
                ExpirationTrackingCache.Remove(cacheKey);
            }
            finally
            {
                sema.Release();
            }

            //object o = _lockStore.GetLockObject(cacheKey);
            //lock (o)
            //{
            //    if (isExpiration && RequiresExpirationRemoval)
            //    {
            //        DeleteItem(cacheKey);
            //    }
            //    else if (!isExpiration)
            //    {
            //        DeleteItem(cacheKey);
            //    }

            //    //Attempt removal of metaCache
            //    RefetchEntry y;
            //    _meta.TryRemove(cacheKey, out y);
            //}
        }
        public void Remove(string cacheKey)
        {
            Remove(cacheKey, false);
        }

        ///// <summary>
        ///// Registers a function for cache. Locks by key and generates data for return from function
        ///// </summary>
        ///// <param name="cacheKey">Unique Cache Keys</param>
        ///// <param name="getData">Function that returns data to be placed in cache</param>
        ///// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        ///// <param name="refetchLimit">Value indicating refetch behavior. -1: Do not refetch, 0: Unlimited refetch (use with caution), x > 0: Number of times to refetch cached data</param>
        ///// <returns></returns>
        //public object Register(string cacheKey, Func<object> getData, TimeSpan cacheTime, int refetchLimit = -1)
        //{

        //}

        //public Task Refresh(string cacheKey)
        //{
        //    cacheKey = StandardizeCacheKey(cacheKey);

        //    var task = new Task(() =>
        //    {
        //        if (Exists(cacheKey))
        //        {
        //            object o = _lockStore.GetLockObject(cacheKey);
        //            lock (o)
        //            {
        //                var meta = _meta[cacheKey];
        //                throw new NotImplementedException("This needs implementing");
        //                //var data = meta.Item1(); //get new data
        //                //SetItem(cacheKey, data, meta.Item2);
        //            }
        //        }
        //    });
        //    task.Start();
        //    return task;
        //}
        private object RefetchData(RefetchEntry entry)
        {
            if (Voat.Common.Extensions.IsGenericType(entry.GetType(), typeof(RefetchEntryTask<>)))
            {
                return ((dynamic)entry).GetData();
            }
            else if (Voat.Common.Extensions.IsGenericType(entry.GetType(), typeof(RefetchEntryFunc<>)))
            {
                return ((dynamic)entry).GetData();
            }
            return null;
        }
        private void RefetchItem(object key, object value, EvictionReason reason, object state)
        {
            var cacheKey = key.ToString();
            if (_meta.ContainsKey(cacheKey))
            {
                var meta = _meta[cacheKey];
                int refreshLimit = meta.MaxCount;
                int refreshCount = meta.CurrentCount + 1;

                if (refreshLimit == 0 || refreshCount <= refreshLimit)
                {
                    meta.CurrentCount = refreshCount;
                    string msg = String.Format("Refetching cache ({0}) - #{1}", cacheKey, refreshCount);
                    EventLogger.Instance.Log(new LogInformation() { Type = LogType.Debug, Category = "Cache", Message = msg, Origin = Configuration.VoatSettings.Instance.Origin.ToString() });

                    _meta[cacheKey] = meta;

                    //Set Tracker Object
                    AddEvictionTracker(cacheKey, new object(), meta.CacheTime.Subtract(_refreshOffset), new PostEvictionDelegate(RefetchItem));

                    try
                    {
                        var data = RefetchData(meta);
                        SetItem(cacheKey, data, meta.CacheTime);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.Log(ex, VoatSettings.Instance.Origin);
                    }
                }
                else
                {
                    string msg = String.Format("Expiring cache ({0}) - #{1}", cacheKey, refreshCount);
                    EventLogger.Instance.Log(new LogInformation() { Type = LogType.Debug, Category = "Cache", Message = msg, Origin = Configuration.VoatSettings.Instance.Origin.ToString() });
                    //Debug.WriteLine(msg);
                    Remove(cacheKey, true);
                }
            }
        }
        private void ExpireItem(object key, object value, EvictionReason reason, object state)
        {
            try
            {
                string msg = String.Format("Expiring cache ({0})", key.ToString());
                EventLogger.Instance.Log(new LogInformation() { Type = LogType.Debug, Category = "Cache", Message = msg, Origin = Configuration.VoatSettings.Instance.Origin.ToString() });
                //Debug.WriteLine(msg);

                if (reason != EvictionReason.None)
                {
                    Remove(key.ToString());
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.ToString());
                EventLogger.Instance.Log(ex);
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
                            var configSettings = CacheConfigurationSettings.Instance;
                            var setInstance = new Action<CacheConfigurationSettings>(settings => 
                            {
                                try
                                {
                                    var handler = settings.Handler;
                                    if (handler != null)
                                    {
                                        Debug.WriteLine($"CacheHandler.Instance.Construct({handler.Type})");
                                        _instance = handler.Construct<ICacheHandler>();
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
                            });
                                
                            setInstance(configSettings);

                            //reset if update
                            configSettings.OnUpdate += (sender, settings) =>
                            {
                                setInstance(settings);
                            };
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
        private IMemoryCache ExpirationTrackingCache
        {
            get
            {
                //CORE_PORT: This is currently just roughed in to ensure it works, needs expirationscanfrequency value from config
                if (_expirationTrackingCache == null)
                {
                    lock (this)
                    {
                        if (_expirationTrackingCache == null)
                        {
                            var moptions = new MemoryCacheOptions();
                            moptions.ExpirationScanFrequency = TimeSpan.FromSeconds(5); // For unit testing only
                            var ioptions = Options.Create(moptions);
                            var memoryCache = new MonitoredMemoryCache(ioptions);
                            _expirationTrackingCache = memoryCache;
                        }
                    }
                }

                return _expirationTrackingCache;
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
        private void AddEvictionTracker(string key, object item, TimeSpan expirationSpan, PostEvictionDelegate callback)
        {
            if (expirationSpan > TimeSpan.Zero)
            {
                var cache = ExpirationTrackingCache;
                var options = new MemoryCacheEntryOptions()
                {
                    //AbsoluteExpiration = absoluteExpiration,
                    AbsoluteExpirationRelativeToNow = expirationSpan

                };
                var callbackRegistration = new PostEvictionCallbackRegistration();
                callbackRegistration.EvictionCallback = callback;
                callbackRegistration.State = key;
                options.PostEvictionCallbacks.Add(callbackRegistration);

                cache.Set(key, new object(), options);
            }
        }
        #region Generic Interfaces

        /// <summary>
        /// Registers a function for cache. Locks by key and generates data for return from function
        /// </summary>
        /// <param name="key">Unique Cache Keys</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="refetchLimit">Value indicating refetch behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        public T Register<T>(string cacheKey, Func<T> getData, TimeSpan cacheTime, int refetchLimit = -1)
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

                //Log the duration this takes to pull fresh
                using (var duration = new DurationLogger(EventLogger.Instance, new LogInformation()
                {
                    Type = LogType.Debug,
                    Origin = Configuration.VoatSettings.Instance.Origin.ToString(),
                    Category = "Duration",
                    Message = $"Cache Load ({cacheKey})"
                },
                TimeSpan.FromSeconds(1)))
                {
                    var sema = _semaphoreSlimLockStore.GetLockObject(cacheKey);
                    sema.Wait();
                    try
                    {
                        //object o = _lockStore.GetLockObject(cacheKey);
                        //lock (o)
                        //{
                        if (!Exists(cacheKey))
                        {
                            try
                            {
                                var data = getData();
                                if (data != null || data == null && !_ignoreNulls)
                                {
                                    EventLogger.Instance.Log(new LogInformation() { Type = LogType.Debug, Category = "Cache", Message = $"Inserting Cache ({cacheKey})", Origin = Configuration.VoatSettings.Instance.Origin.ToString() });
                                    //Debug.WriteLine("Inserting Cache: " + cacheKey);
                                    SetItem(cacheKey, data, cacheTime);

                                    //Refetch Logic
                                    if (RefetchEnabled && refetchLimit >= 0)
                                    {
                                        _meta[cacheKey] = new RefetchEntryFunc<T>(getData) { CacheTime = cacheTime, CurrentCount = 0, MaxCount = refetchLimit };

                                        AddEvictionTracker(cacheKey, new object(), cacheTime.Subtract(_refreshOffset), new PostEvictionDelegate(RefetchItem));

                                    }
                                    else if (RequiresExpirationRemoval)
                                    {
                                        AddEvictionTracker(cacheKey, new object(), cacheTime, new PostEvictionDelegate(ExpireItem));
                                        //AddEvictionTracker(cacheKey, new object(), Repository.CurrentDate.Add(cacheTime), new PostEvictionDelegate(ExpireItem));
                                        //cache.Add(cacheKey, new object(), new CacheItemPolicy() { AbsoluteExpiration = Repository.CurrentDate.Add(cacheTime), RemovedCallback = new CacheEntryRemovedCallback(ExpireItem) });
                                    }
                                }
                                return data;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                        //}
                    }
                    finally
                    {
                        sema.Release();
                    }
                }
            }
            var cacheItem = GetItem(cacheKey);
            return Convert<T>(cacheItem);
        }

        /// <summary>
        /// Registers a function for cache. Locks by key and generates data for return from function
        /// </summary>
        /// <param name="key">Unique Cache Keys</param>
        /// <param name="getData">Function that returns data to be placed in cache</param>
        /// <param name="cacheTime">The timespan in which to update or remove item from cache</param>
        /// <param name="refetchLimit">Value indicating refresh behavior. -1: Do not refresh, 0: Unlimited refresh (use with caution), x > 0: Number of times to refresh cached data</param>
        /// <returns></returns>
        public async Task<T> Register<T>(string cacheKey, Func<Task<T>> getData, TimeSpan cacheTime, int refetchLimit = -1)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            //allow devs to turn off local cache for testing
            if (!CacheEnabled)
            {
                return await getData();
            }

            if (!Exists(cacheKey))
            {
                //Log the duration this takes to pull fresh
                using (var duration = new DurationLogger(EventLogger.Instance,
                    new LogInformation()
                    {
                        Type = LogType.Debug,
                        Origin = Configuration.VoatSettings.Instance.Origin.ToString(),
                        Category = "Duration",
                        Message = $"Cache Load ({cacheKey})"
                    }, TimeSpan.FromSeconds(1)))
                {
                    var o = _semaphoreSlimLockStore.GetLockObject(cacheKey);
                    await o.WaitAsync();
                    try
                    {
                        if (!Exists(cacheKey))
                        {
                            try
                            {
                                var data = await getData().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                                if (data != null || data == null && !_ignoreNulls)
                                {
                                    EventLogger.Instance.Log(new LogInformation() { Type = LogType.Debug, Category = "Cache", Message = $"Inserting Cache ({cacheKey})", Origin = Configuration.VoatSettings.Instance.Origin.ToString() });
                                    //Debug.WriteLine("Inserting Cache: " + cacheKey);
                                    SetItem(cacheKey, data, cacheTime);

                                    //Refetch Logic
                                    if (RefetchEnabled && refetchLimit >= 0)
                                    {
                                        _meta[cacheKey] = new RefetchEntryTask<T>(getData) { CacheTime = cacheTime, CurrentCount = 0, MaxCount = refetchLimit };

                                        AddEvictionTracker(cacheKey, new object(), cacheTime.Subtract(_refreshOffset), new PostEvictionDelegate(RefetchItem));
                                        //AddEvictionTracker(cacheKey, new object(), Repository.CurrentDate.Add(cacheTime.Subtract(_refreshOffset)), new PostEvictionDelegate(RefetchItem));

                                        //var cache = System.Runtime.Caching.MemoryCache.Default;
                                        //var policy = new CacheItemPolicy()
                                        //{
                                        //    AbsoluteExpiration = Repository.CurrentDate.Add(cacheTime.Subtract(_refreshOffset)),
                                        //    UpdateCallback = new CacheEntryUpdateCallback(RefetchItem)
                                        //};
                                        //cache.Set(cacheKey, new object(), policy);

                                    }
                                    else if (RequiresExpirationRemoval)
                                    {
                                        AddEvictionTracker(cacheKey, new object(), cacheTime, new PostEvictionDelegate(ExpireItem));
                                        //AddEvictionTracker(cacheKey, new object(), Repository.CurrentDate.Add(cacheTime), new PostEvictionDelegate(ExpireItem));
                                        //var cache = System.Runtime.Caching.MemoryCache.Default;
                                        //cache.Add(cacheKey, new object(), new CacheItemPolicy() { AbsoluteExpiration = Repository.CurrentDate.Add(cacheTime), RemovedCallback = new CacheEntryRemovedCallback(ExpireItem) });

                                    }
                                }
                                return data;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                    finally
                    {
                        o.Release();
                    }
                }
            }
            var cacheItem = GetItem(cacheKey);
            return Convert<T>(cacheItem);
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

        public virtual void DictionaryReplace<K, V>(string cacheKey, K key, Func<V, V> replaceAlg, bool bypassMissing = true)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            var sema = _semaphoreSlimLockStore.GetLockObject(cacheKey);
            sema.Wait();
            try
            {
                V item = (V)GetItem<K, V>(cacheKey, key, CacheType.Dictionary);
                if (!bypassMissing || (bypassMissing && !item.IsDefault()))
                {
                    SetItem(cacheKey, key, replaceAlg(item), CacheType.Dictionary);
                }
            }
            finally
            {
                sema.Release();
            }

            //lock (_lockStore.GetLockObject(cacheKey))
            //{
            //    V item = (V)GetItem<K,V>(cacheKey, key, CacheType.Dictionary);
            //    if (!bypassMissing || (bypassMissing && !item.IsDefault())) 
            //    {
            //        SetItem(cacheKey, key, replaceAlg(item), CacheType.Dictionary);
            //    }
            //}
        }

        public virtual void DictionaryRemove<K>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            DeleteItem(cacheKey, key, CacheType.Dictionary);
        }

        public virtual void DictionaryReplace<K, V>(string cacheKey, K key, V newObject)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            SetItem(cacheKey, key, newObject, CacheType.Dictionary);
        }

        public virtual V DictionaryRetrieve<K, V>(string cacheKey, K key)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return (V)GetItem<K, V>(cacheKey, key, CacheType.Dictionary);
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
        public abstract int SetLength(string cacheKey);

        #endregion

        #region List

        public abstract void ListAdd<T>(string cacheKey, T item);

        public abstract T ListRetrieve<T>(string cacheKey, int index);

        public abstract int ListLength(string cacheKey);

        public abstract IEnumerable<T> ListRetrieveAll<T>(string cacheKey);

        #endregion

    }
}
