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
            if (_cache.ContainsKey(cacheKey))
            {
                var value = _cache[cacheKey];
                if (value is IDictionary)
                {
                    return (V)((IDictionary)value)[key];
                }
                else if (value is IList)
                {
                    var list = ((IList)value);
                    var index = list.IndexOf(key);
                    return (V)list[index];
                }
                else
                {
                    throw new ArgumentException(String.Format("Cache item {0} is not a supported type", cacheKey));
                }
            }
            return default(V);
        }
        protected override void SetItem<K,V>(string cacheKey, K key, V item, CacheType type)
        {
            if (_cache.ContainsKey(cacheKey))
            {
                var value = _cache[cacheKey];
                if (value is IDictionary)
                {
                    ((IDictionary)value)[key] = item;
                    return;
                }
                else if (value.GetType().HasInterface(typeof(ISet<>)))
                {
                    ((dynamic)value).Add((dynamic)key);
                    return;
                }
                throw new ArgumentException(String.Format("Cache item {0} is not a dictionary", cacheKey));
            }
            //else
            //{
            //    var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(object), item.GetType()));

            //    dict.Add(key, item);
            //    _cache[cacheKey] = dict;
            //}
        }
        protected override void DeleteItem<T>(string cacheKey, T key, CacheType type)
        {
            var value = _cache[cacheKey];
            if (value != null)
            {
                switch (type)
                {
                    case CacheType.Dictionary:
                        ((IDictionary)value).Remove(key);
                        break;
                    case CacheType.Set:
                        ((ISet<T>)value).Remove(key);
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
            if (_cache.ContainsKey(cacheKey))
            {
                var value = _cache[cacheKey];
                switch (type)
                {
                    case CacheType.Dictionary:
                        found = ((IDictionary)value).Contains(key);
                        break;
                    case CacheType.Set:
                        found = ((ISet<T>)value).Contains(key);
                        break;
                }
                //var value = _cache[cacheKey];
                //if (value is IDictionary)
                //{
                //    return ((IDictionary)value).Contains(key);
                //}
                //else if (value is IList)
                //{
                //    return ((IList)value).Contains(key);
                //}
                //else if (value.GetType().HasInterface(typeof(ISet<T>)))
                //{
                //    return ((dynamic)value).Contains((dynamic)key);
                //}
            }
            return found;
        }

        protected override void ProtectedPurge()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }
    }
}
