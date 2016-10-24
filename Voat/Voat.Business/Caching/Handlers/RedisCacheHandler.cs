using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Voat.Caching
{
    public class RedisCacheHandler : CacheHandler
    {
        private string _connectionString;
        protected ConnectionMultiplexer conn = null;
        protected IDatabase db = null;

        //This ensures that an item eventually gets purged from cache.
        private TimeSpan expirationBuffer = TimeSpan.FromSeconds(30);

        private JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public RedisCacheHandler(string connectionString)
        {
            _connectionString = connectionString;

            //TODO: Pull endpoint from configuration
            conn = ConnectionMultiplexer.Connect(connectionString);

            db = conn.GetDatabase();
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
            if (db.KeyExists(cacheKey))
            {
                RedisType type = db.KeyType(cacheKey);
                if (type == RedisType.Hash)
                {
                    var entries = db.HashGetAll(cacheKey);
                    return entries.ToDictionary<HashEntry, object, object>(new Func<HashEntry, object>(x => x.Name), new Func<HashEntry, object>(x => Deserialize(x.Value)));
                }
                else if (type == RedisType.Set || type == RedisType.SortedSet)
                {
                    throw new InvalidOperationException("Set based values do not support GetItem");
                }
                else
                {
                    var val = db.StringGet(cacheKey);
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
            db.KeyDelete(cacheKey);
        }

        protected override bool ItemExists(string cacheKey)
        {
            return db.KeyExists(cacheKey);
        }
        protected override V GetItem<K, V>(string cacheKey, K key, CacheType type)
        {
            V returnVal = default(V);
            switch (type)
            {
                case CacheType.Dictionary:
                    if (db.HashExists(cacheKey, key.ToString()))
                    {
                        returnVal = (V)Deserialize(db.HashGet(cacheKey, key.ToString()));
                    }
                    break;
            }
            return returnVal;
        }

        protected override void SetItem<K,V>(string cacheKey, K key, V item, CacheType type)
        {
            if (db.KeyExists(cacheKey))
            {
                switch (type)
                {
                    case CacheType.Dictionary:
                        db.HashSet(cacheKey, new HashEntry[] { new HashEntry(key.ToString(), Serialize(item)) });
                        break;
                    case CacheType.Set:
                        db.SetAdd(cacheKey, Serialize(key));
                        break;
                }
            }
        }

        protected override void SetItem(string cacheKey, object item, TimeSpan? cacheTime = null)
        {
            //determine if dictionary
            if (item is IDictionary)
            {
                var dict = (IDictionary)item;
                var hashes = new List<HashEntry>();
                foreach (object key in dict.Keys)
                {
                    hashes.Add(new HashEntry(key.ToString(), Serialize(dict[key])));
                }
                db.HashSet(cacheKey, hashes.ToArray());
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
                    db.SetAdd(cacheKey, values.ToArray());
                }
            }
            else
            {
                if (db.KeyExists(cacheKey))
                {
                    var type = db.KeyType(cacheKey);
                    if (type == RedisType.Hash)
                    {
                        throw new InvalidOperationException("SetItem can not add dictionary entry without key");
                    }
                    else if (type == RedisType.Set || type == RedisType.SortedSet)
                    {
                        db.SetAdd(cacheKey, Serialize(item));
                    }
                }
                else
                {
                    db.StringSet(cacheKey, Serialize(item));
                }
            }
            if (cacheTime.HasValue)
            {
                db.KeyExpire(cacheKey, cacheTime.Value.Add(expirationBuffer));
            }
        }
        protected override void DeleteItem<T>(string cacheKey, T key, CacheType type)
        {
            switch (type)
            {
                case CacheType.Dictionary:
                    db.HashDelete(cacheKey, key.ToString());
                    break;
                case CacheType.Set:
                    db.SetRemove(cacheKey, Serialize(key));
                    break;
            }
        }

        protected override bool ItemExists<T>(string cacheKey, T key, CacheType type)
        {
            var found = false;
            switch (type)
            {
                case CacheType.Dictionary:
                    found = db.HashExists(cacheKey, key.ToString());
                    break;
                case CacheType.Set:
                    found = db.SetContains(cacheKey, Serialize(key));
                    break;
            }
            return found;
        }

        protected override void ProtectedPurge()
        {
            var endpoint = conn.GetEndPoints().First();
            var server = conn.GetServer(endpoint);
            server.FlushDatabase(db.Database);
        }
    }
}
