using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _cache[cacheKey] = item;
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

        protected override object GetItem(string cacheKey, object dictionaryKey)
        {
            var value = _cache[cacheKey];
            if (value is IDictionary)
            {
                return ((IDictionary)value)[dictionaryKey];
            }
            throw new ArgumentException(String.Format("Cache item {0} is not a dictionary", cacheKey));
        }

        protected override void SetItem(string cacheKey, object dictionaryKey, object item)
        {
            if (_cache.ContainsKey(cacheKey))
            {
                var value = _cache[cacheKey];
                if (value is IDictionary)
                {
                    ((IDictionary)value)[dictionaryKey] = item;
                    return;
                }
                throw new ArgumentException(String.Format("Cache item {0} is not a dictionary", cacheKey));
            }
            else
            {
                var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(object), item.GetType()));

                dict.Add(dictionaryKey, item);
                _cache[cacheKey] = dict;
            }

        }

        protected override void DeleteItem(string cacheKey, object dictionaryKey)
        {
            var value = _cache[cacheKey];
            if (value is IDictionary)
            {
                ((IDictionary)value).Remove(dictionaryKey);
                return;
            }

            throw new ArgumentException(String.Format("Cache item {0} is not a dictionary", cacheKey));
        }

        protected override bool ItemExists(string cacheKey, object dictionaryKey)
        {
            if (_cache.ContainsKey(cacheKey))
            {
                var value = _cache[cacheKey];
                if (value is IDictionary)
                {
                    return ((IDictionary)value).Contains(dictionaryKey);
                }
            }
            return false;
        }
        protected override void ProtectedPurge()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }
    }
}
