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
using System.Collections.Generic;
using System.Linq;

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
        public override K ListRetrieve<K>(string cacheKey, int index)
        {
            return default(K);
        }
        public override int ListLength(string cacheKey)
        {
            return 0;
        }
        public override IEnumerable<T> ListRetrieveAll<T>(string cacheKey)
        {
            return Enumerable.Empty<T>();
        }
        public override void ListAdd<K>(string cacheKey, K key)
        {
                
        }
        protected override void ProtectedPurge()
        {
        }
        public override int SetLength(string cacheKey)
        {
            return 0;
        }
    }
}
