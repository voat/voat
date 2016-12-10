using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Voat.Caching
{
    public class MemoryCacheHandler : CacheHandler
    {
        //data kept in dictionary to serve as hot cache
        private ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public MemoryCacheHandler()
        {
            RequiresExpirationRemoval = true;
            base.Initialize();
        }

        protected override object GetItem(string cacheKey)
        {
            return _cache[cacheKey];
        }

        protected override void SetItem(string cacheKey, object item, TimeSpan? cacheTime = null)
        {
            if (ItemExists(cacheKey))
            {
                var value = GetItem(cacheKey);
                if (value.GetType().HasInterface(typeof(ISet<>)))
                {
                    ((dynamic)value).Add((dynamic)item);
                }
                else
                {
                    _cache[cacheKey] = item;
                }
            }
            else
            {
                _cache[cacheKey] = item;
            }
        }

        protected override void DeleteItem(string cacheKey)
        {
            object x;
            _cache.TryRemove(cacheKey, out x);
        }

        protected override bool ItemExists(string cacheKey)
        {
           return _cache.ContainsKey(cacheKey);
        }
        protected override V GetItem<K,V>(string cacheKey, K key, CacheType type)
        {
            if (ItemExists(cacheKey))
            {
                var value = _cache[cacheKey];
                switch (type)
                {
                    case CacheType.Dictionary:
                        return (V)(value.Convert<IDictionary, object>())[key];
                        break;
                    default:
                        throw new ArgumentException(String.Format("Cache item {0} is not a supported type", cacheKey));
                }
            }
            return default(V);
        }
        protected override void SetItem<K,V>(string cacheKey, K key, V item, CacheType type)
        {
            if (ItemExists(cacheKey))
            {
                var value = _cache[cacheKey];
                switch (type)
                {
                    case CacheType.Dictionary:
                        value.Convert<IDictionary, object>()[key] = item;
                        break;
                    case CacheType.Set:
                        var set = value.Convert<ISet<K>, object>();
                        if (!set.Contains(key))
                        {
                            set.Add(key);
                        }
                        break;
                }
            }
        }
        protected override void DeleteItem<T>(string cacheKey, T key, CacheType type)
        {
            if (ItemExists(cacheKey))
            {
                var value = _cache[cacheKey];
                switch (type)
                {
                    case CacheType.Dictionary:
                        value.Convert<IDictionary, object>().Remove(key);
                        break;
                    case CacheType.Set:
                        value.Convert<ISet<T>, object>().Remove(key);
                        break;
                    default:
                        throw new ArgumentException(String.Format("Operation on type {0} not supported", type));
                        break;
                }

            }
        }
        protected override bool ItemExists<T>(string cacheKey, T key, CacheType type)
        {
            var found = false;
            if (ItemExists(cacheKey))
            {
                var value = _cache[cacheKey];
                switch (type)
                {
                    case CacheType.Dictionary:
                        found = value.Convert<IDictionary, object>().Contains(key);
                        break;
                    case CacheType.Set:
                        found = value.Convert<ISet<T>, object>().Contains(key);
                        break;
                }
            }
            return found;
        }

        protected override void ProtectedPurge()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }
    }
}
