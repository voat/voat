using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            else
            {
                db.StringSet(cacheKey, Serialize(item));
            }
            if (cacheTime.HasValue)
            {
                db.KeyExpire(cacheKey, cacheTime.Value.Add(expirationBuffer));
            }
        }

        protected override void DeleteItem(string cacheKey)
        {
            db.KeyDelete(cacheKey);
        }

        protected override bool ItemExists(string cacheKey)
        {
            return db.KeyExists(cacheKey);
        }

        protected override object GetItem(string cacheKey, object dictionaryKey)
        {
            if (db.HashExists(cacheKey, dictionaryKey.ToString()))
            {
                return Deserialize(db.HashGet(cacheKey, dictionaryKey.ToString()));
            }
            return null;
        }

        protected override void SetItem(string cacheKey, object dictionaryKey, object item)
        {
            db.HashSet(cacheKey, new HashEntry[] { new HashEntry(dictionaryKey.ToString(), Serialize(item)) });
        }

        protected override void DeleteItem(string cacheKey, object dictionaryKey)
        {
            db.HashDelete(cacheKey, dictionaryKey.ToString());
        }

        protected override bool ItemExists(string cacheKey, object dictionaryKey)
        {
            return db.HashExists(cacheKey, dictionaryKey.ToString());
        }

        protected override void ProtectedPurge()
        {
            var endpoint = conn.GetEndPoints().First();
            var server = conn.GetServer(endpoint);
            server.FlushDatabase(db.Database);
        }
    }
}
