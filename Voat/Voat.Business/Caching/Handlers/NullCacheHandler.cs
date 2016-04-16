using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Caching
{
    /// <summary>
    /// This class is used to plug in no-cache behavior in Voat.
    /// </summary>
    public class NullCacheHandler : CacheHandler
    {
        public NullCacheHandler()
        {
            base.CacheEnabled = false;
        }

        protected override object GetItem(string key)
        {
            return null;
        }

        protected override void SetItem(string key, object item, TimeSpan? cacheTime = null)
        {

        }

        protected override void DeleteItem(string key)
        {

        }

        protected override bool ItemExists(string key)
        {
            return false;
        }

        protected override object GetItem(string cacheKey, object dictionaryKey)
        {
            return null;
        }

        protected override void SetItem(string cacheKey, object dictionaryKey, object item)
        {

        }

        protected override void DeleteItem(string cacheKey, object dictionaryKey)
        {

        }

        protected override bool ItemExists(string cacheKey, object dictionaryKey)
        {
            return false;
        }

        protected override void ProtectedPurge()
        {

        }
    }
}
