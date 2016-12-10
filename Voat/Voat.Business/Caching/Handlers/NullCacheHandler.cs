using System;

namespace Voat.Caching
{
    /// <summary>
    /// This class is used to plug in no-cache behavior in Voat.
    /// </summary>
    public class NullCacheHandler : CacheHandler
    {
        public NullCacheHandler()
        {
            base.RequiresExpirationRemoval = false;
            base.CacheEnabled = false;
            base.Initialize();
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
        protected override V GetItem<K,V>(string cacheKey, K key, CacheType type)
        {
            return default(V);
        }
        protected override void SetItem<K,V>(string cacheKey, K key, V item, CacheType type)
        {
            
        }
        protected override void DeleteItem<K>(string cacheKey, K key, CacheType type)
        {
            
        }
        protected override bool ItemExists<K>(string cacheKey, K key, CacheType type)
        {
            return false;
        }
        protected override void ProtectedPurge()
        {
        }
    }
}
