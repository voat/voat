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

using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Voat.Common;
using Voat.Configuration;

namespace Voat.Caching
{
    public class RedisCacheHandler : CacheHandler
    {
        public enum ConnectionType
        {
            Read,
            Write,
            Exists
        }

        private Dictionary<ConnectionType, ConnectionMultiplexer> connections = new Dictionary<ConnectionType, ConnectionMultiplexer>();
        private string _connectionString;

        //This ensures that an item eventually gets purged from cache.
        private TimeSpan expirationBuffer = TimeSpan.FromSeconds(30);

        private JsonSerializerSettings settings =  JsonSettings.DataSerializationSettings;
        public IDatabase GetDatabase(ConnectionType type)
        {
            return connections[type].GetDatabase();
        }

        public RedisCacheHandler(string connectionString, bool refetchEnabled = true) : base(refetchEnabled)
        {
            _connectionString = connectionString;
            connections.Add(ConnectionType.Read, ConnectionMultiplexer.Connect(connectionString));
            connections.Add(ConnectionType.Write, ConnectionMultiplexer.Connect(connectionString));
            connections.Add(ConnectionType.Exists, ConnectionMultiplexer.Connect(connectionString));
            base.RequiresExpirationRemoval = false;
            base.Initialize();
        }

        protected object Deserialize(object value)
        {
            return JsonConvert.DeserializeObject(value.ToString(), settings);
        }

        protected string Serialize(object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, Formatting.None, settings);
        }

        protected override object GetItem(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            if (GetDatabase(ConnectionType.Exists).KeyExists(cacheKey))
            {
                RedisType type = GetDatabase(ConnectionType.Read).KeyType(cacheKey);
                if (type == RedisType.Hash)
                {
                    var entries = GetDatabase(ConnectionType.Read).HashGetAll(cacheKey);
                    return entries.ToDictionary<HashEntry, object, object>(new Func<HashEntry, object>(x => x.Name), new Func<HashEntry, object>(x => Deserialize(x.Value)));
                }
                else if (type == RedisType.Set || type == RedisType.SortedSet)
                {
                    throw new InvalidOperationException($"Set based values do not support GetItem. CacheKey: {cacheKey}");
                }
                else
                {
                    var val = GetDatabase(ConnectionType.Read).StringGet(cacheKey);
                    if (val.HasValue)
                    {
                        return Deserialize(val);
                    }
                }
            }
            return null;
        }


        protected override void DeleteItem(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            GetDatabase(ConnectionType.Write).KeyDelete(cacheKey);
        }

        protected override bool ItemExists(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            return GetDatabase(ConnectionType.Exists).KeyExists(cacheKey);
        }
        protected override V GetItem<K, V>(string cacheKey, K key, CacheType type)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            V returnVal = default(V);
            switch (type)
            {
                case CacheType.Dictionary:
                    if (GetDatabase(ConnectionType.Exists).HashExists(cacheKey, key.ToString()))
                    {
                        returnVal = (V)Deserialize(GetDatabase(ConnectionType.Read).HashGet(cacheKey, key.ToString()));
                    }
                    break;
            }
            return returnVal;
        }

        protected override void SetItem<K,V>(string cacheKey, K key, V item, CacheType type)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            if (GetDatabase(ConnectionType.Exists).KeyExists(cacheKey))
            {
                switch (type)
                {
                    case CacheType.Dictionary:
                        GetDatabase(ConnectionType.Write).HashSet(cacheKey, new HashEntry[] { new HashEntry(key.ToString(), Serialize(item)) });
                        break;
                    case CacheType.Set:
                        GetDatabase(ConnectionType.Write).SetAdd(cacheKey, Serialize(key));
                        break;
                    case CacheType.List:
                        GetDatabase(ConnectionType.Write).ListRightPush(cacheKey, Serialize(key));
                        break;
                }
            }
        }

        protected override void SetItem(string cacheKey, object item, TimeSpan? cacheTime = null)
        {
            cacheKey = StandardizeCacheKey(cacheKey);

            //determine if dictionary
            if (item is IDictionary)
            {
                var dict = (IDictionary)item;
                var hashes = new List<HashEntry>();
                foreach (object key in dict.Keys)
                {
                    hashes.Add(new HashEntry(key.ToString(), Serialize(dict[key])));
                }
                GetDatabase(ConnectionType.Write).HashSet(cacheKey, hashes.ToArray());
            }
            else if (item.GetType().HasInterface(typeof(ISet<>)))
            {
                var list = (IEnumerable)item;
                var values = new List<RedisValue>();
                foreach (object o in list)
                {
                    values.Add(Serialize(o));
                }
                if (values.Count > 0)
                {
                    GetDatabase(ConnectionType.Write).SetAdd(cacheKey, values.ToArray());
                }
            }
            else
            {
                if (GetDatabase(ConnectionType.Exists).KeyExists(cacheKey))
                {
                    var type = GetDatabase(ConnectionType.Read).KeyType(cacheKey);
                    if (type == RedisType.Hash)
                    {
                        throw new InvalidOperationException("SetItem can not add dictionary entry without key");
                    }
                    else if (type == RedisType.Set || type == RedisType.SortedSet)
                    {
                        GetDatabase(ConnectionType.Write).SetAdd(cacheKey, Serialize(item));
                    }
                    else
                    {
                        GetDatabase(ConnectionType.Write).StringSet(cacheKey, Serialize(item));
                    }
                }
                else
                {
                    GetDatabase(ConnectionType.Write).StringSet(cacheKey, Serialize(item));
                }
            }
            if (cacheTime.HasValue)
            {
                GetDatabase(ConnectionType.Write).KeyExpire(cacheKey, cacheTime.Value.Add(expirationBuffer));
            }
        }
        protected override void DeleteItem<T>(string cacheKey, T key, CacheType type)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            switch (type)
            {
                case CacheType.Dictionary:
                    GetDatabase(ConnectionType.Write).HashDelete(cacheKey, key.ToString());
                    break;
                case CacheType.Set:
                    GetDatabase(ConnectionType.Write).SetRemove(cacheKey, Serialize(key));
                    break;
                case CacheType.List:
                    GetDatabase(ConnectionType.Write).ListRemove(cacheKey, Serialize(key));
                    break;
            }
        }

        protected override bool ItemExists<T>(string cacheKey, T key, CacheType type)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            var found = false;
            switch (type)
            {
                case CacheType.Dictionary:
                    found = GetDatabase(ConnectionType.Exists).HashExists(cacheKey, key.ToString());
                    break;
                case CacheType.Set:
                    found = GetDatabase(ConnectionType.Exists).SetContains(cacheKey, Serialize(key));
                    break;
            }
            return found;
        }
        public override int SetLength(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return (int)GetDatabase(ConnectionType.Read).SetLength(cacheKey);
        }
        public override void ListAdd<K>(string cacheKey, K item)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            GetDatabase(ConnectionType.Exists).ListRightPush(cacheKey, Serialize(item)); 
        }

        public override K ListRetrieve<K>(string cacheKey, int index)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            long longIndex = index;
            var value = GetDatabase(ConnectionType.Exists).ListGetByIndex(cacheKey, longIndex);
            if (value.HasValue)
            {
                return (K)Deserialize(value);
            }
            return default(K);
        }
        public override int ListLength(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            return (int)GetDatabase(ConnectionType.Exists).ListLength(cacheKey);
        }
        public override IEnumerable<T> ListRetrieveAll<T>(string cacheKey)
        {
            cacheKey = StandardizeCacheKey(cacheKey);
            var range = GetDatabase(ConnectionType.Exists).ListRange(cacheKey, 0);
            if (range != null && range.Any())
            {
                return range.Select(x => (T)Deserialize(x));
            }
            return Enumerable.Empty<T>();
        }


        protected override void ProtectedPurge()
        {
            
            var conn = connections[ConnectionType.Write];
            var endpoint = conn.GetEndPoints().First();
            var server = conn.GetServer(endpoint);
            server.FlushDatabase(conn.GetDatabase().Database);
        }
    }
}
