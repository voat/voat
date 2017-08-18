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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Voat.Common;

namespace Voat.Caching
{
    public class MemoryCacheHandler : CacheHandler
    {
        //data kept in dictionary to serve as hot cache
        private ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        public MemoryCacheHandler(bool refetchEnabled = true) : base(refetchEnabled)
        {
            RequiresExpirationRemoval = true;
            base.Initialize();
        }

        protected override object GetItem(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            return _cache[cacheKey];
        }

        protected override void SetItem(string cacheKey, object item, TimeSpan? cacheTime = null)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

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
            cacheKey = StandardizeCacheKey(cacheKey);

            object x;
            _cache.TryRemove(cacheKey, out x);
        }

        protected override bool ItemExists(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            return _cache.ContainsKey(cacheKey);
        }
        protected override V GetItem<K,V>(string cacheKey, K key, CacheType type)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

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
            cacheKey = StandardizeCacheKey(cacheKey);

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
            cacheKey = StandardizeCacheKey(cacheKey);

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
                    case CacheType.List:
                        value.Convert<IList<T>, object>().Remove(key);
                        break;
                    default:
                        throw new ArgumentException(String.Format("Operation on type {0} not supported", type));
                        break;
                }

            }
        }
        protected override bool ItemExists<T>(string cacheKey, T key, CacheType type)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

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
        public override void ListAdd<T>(string cacheKey, T item)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            IList<T> list = null;
            if (_cache.ContainsKey(cacheKey))
            {
                list = _cache[cacheKey].Convert<IList<T>>();
            }
            else
            {
                list = new List<T>();
                _cache[cacheKey] = list; 
            }
            list.Add(item);
        }
        public override T ListRetrieve<T>(string cacheKey, int index)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            var result = default(T);
            if (_cache.ContainsKey(cacheKey))
            {
                result = _cache[cacheKey].Convert<IList<T>>()[index];
            }
            return result;
        }
        public override int ListLength(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            var result = 0;
            if (_cache.ContainsKey(cacheKey))
            {
                result = _cache[cacheKey].Convert<IList>().Count;
            }
            return result;
        }
        public override IEnumerable<T> ListRetrieveAll<T>(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            IEnumerable<T> result = Enumerable.Empty<T>();
            
            if (_cache.ContainsKey(cacheKey))
            {
                result = _cache[cacheKey].Convert<IList<T>>();
            }
            return result;
        }

        public override int SetLength(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            var result = 0;
            if (_cache.ContainsKey(cacheKey))
            {
                //Warning: This is a hack. Be warned.
                dynamic item = _cache[cacheKey];
                result = item.Count;
            }
            return result;
        }

        protected override void ProtectedPurge()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }
    }
}
