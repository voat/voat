#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Voat.Caching;
using Voat.Domain.Models;

namespace Voat.Tests.Cache
{
    public class CacheTests
    {
        public ICacheHandler handler = null;

        public CacheTests(ICacheHandler handler)
        {
            this.handler = handler;
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        public void CustomObject_Add()
        {
            string key = DateTime.UtcNow.ToOADate().ToString();

            handler.Register<Item>(key, () => new Item() { ID = 1, Name = "Bill" }, TimeSpan.FromSeconds(30));

            Assert.AreEqual(true, handler.Exists(key));
            var val = handler.Retrieve<Item>(key);
            Assert.IsNotNull(val);
            Assert.AreEqual(1, val.ID);
            Assert.AreEqual("Bill", val.Name);
            handler.Remove(key);
            Assert.AreEqual(false, handler.Exists(key));
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        public void Int_Add()
        {
            string key = DateTime.UtcNow.ToOADate().ToString();

            handler.Register<long>(key, () => 1, TimeSpan.FromSeconds(30));

            Assert.AreEqual(true, handler.Exists(key));
            var val = handler.Retrieve<long>(key);
            Assert.AreEqual(1, val);
            handler.Remove(key);
            Assert.AreEqual(false, handler.Exists(key));
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        public void Purge_Cache()
        {
            handler.Purge();
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_Add()
        {
            string cacheKey = "TestDictionary_Add";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));
            //handler.RegisterDictionary(cacheKey, new Func<IList<Item>>(() => items), new Func<Item, object>((x) => x.ID), TimeSpan.FromMinutes(100));
            var r = handler.Retrieve<IDictionary>(cacheKey);

            Assert.IsNotNull(r);
            Assert.AreEqual(items.Count, r.Count);

            var item = handler.Retrieve<Item>(cacheKey, 1);
            Assert.IsNotNull(item);
            Assert.AreEqual(1, item.ID);
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_RemoveExisting()
        {
            string cacheKey = "TestDictionary_RemoveExisting";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));

            //replace existing key
            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.Remove(cacheKey, 1);

            var cachedItem = handler.Retrieve<Item>(cacheKey, 1);

            Assert.IsNull(cachedItem);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_RemoveNonExisting()
        {
            string cacheKey = "TestDictionary_RemoveNonExisting";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));

            //replace existing key
            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.Remove(cacheKey, 20);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_ReplaceExisting()
        {
            string cacheKey = "TestDictionary_ReplaceExisting";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));

            //replace existing key
            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.Replace(cacheKey, 1, newItem);

            var cachedItem = handler.Retrieve<Item>(cacheKey, 1);

            Assert.IsNotNull(cachedItem);
            Assert.AreEqual(newItem.ID, cachedItem.ID);
            Assert.AreEqual(newItem.Name, cachedItem.Name);
            Assert.AreEqual(newItem.Count, cachedItem.Count);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_ReplaceWithoutPrevious()
        {
            string cacheKey = "TestDictionary_Add";
            handler.Remove(cacheKey);

            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.Replace<Item>(cacheKey, 1, newItem);
            Assert.IsTrue(handler.Exists(cacheKey, 1));

            var newItem2 = new Item() { ID = 2, Count = 400, Name = "New Name" };
            handler.Replace(cacheKey, 2, newItem2);
            Assert.IsTrue(handler.Exists(cacheKey, 2));

            var cacheItem = handler.Retrieve<Item>(cacheKey, 1);
            Assert.IsNotNull(cacheItem);
            Assert.AreEqual(newItem.ID, cacheItem.ID);
            Assert.AreEqual(newItem.Count, cacheItem.Count);
            Assert.AreEqual(newItem.Name, cacheItem.Name);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_TypedDictionary_EnumKey_AddItem()
        {
            string cacheKey = "TestDictionary_TypedDictionary_EnumKey_AddItem";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<DomainType, IList<Item>>>(() =>
            {
                var dictionary = new Dictionary<DomainType, IList<Item>> { { DomainType.Subverse, items }, { DomainType.Set, items }, { DomainType.User, new List<Item>() } };
                return dictionary;
            }), TimeSpan.FromMinutes(30));

            var dict = handler.Retrieve<IDictionary<DomainType, IList<Item>>>(cacheKey);
            Assert.IsNotNull(dict);

            handler.Replace(cacheKey, DomainType.User, new List<Item>() { new Item() { ID = 20, Count = 20, Name = "Twenty" } });

            //See if we can get updated dictionary entry by enum key
            var data = handler.Retrieve<IList<Item>>(cacheKey, DomainType.User);
            var item = data.First();
            Assert.IsNotNull(item);
            Assert.AreEqual(20, item.ID);
            Assert.AreEqual("Twenty", item.Name);

            dict = handler.Retrieve<IDictionary<DomainType, IList<Item>>>(cacheKey);
            Assert.IsNotNull(dict);
            Assert.AreEqual(3, dict.Count);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_TypedDictionary_IntKey_AddItem()
        {
            string cacheKey = "TestDictionary_TypedDictionary_IntKey_AddItem";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<int, Item>>(() => items.ToDictionary(x => x.ID)), TimeSpan.FromMinutes(30));

            var dict = handler.Retrieve<IDictionary<int, Item>>(cacheKey);
            Assert.IsNotNull(dict);

            handler.Replace(cacheKey, 20, new Item() { ID = 20, Count = 20, Name = "Twenty" });

            //See if we can get dictionary entry by int key
            var item = handler.Retrieve<Item>(cacheKey, 20);
            Assert.IsNotNull(item);
            Assert.AreEqual(20, item.ID);
            Assert.AreEqual("Twenty", item.Name);

            dict = handler.Retrieve<IDictionary<int, Item>>(cacheKey);
            Assert.IsNotNull(dict);
            Assert.AreEqual(11, dict.Count);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_TypedDictionary_IntKey_Read()
        {
            string cacheKey = "TestDictionary_TypedDictionary_IntKey_Read";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<int, Item>>(() => items.ToDictionary(x => x.ID)), TimeSpan.FromMinutes(30));

            var dict = handler.Retrieve<IDictionary<int, Item>>(cacheKey);
            Assert.IsNotNull(dict);

            //See if we can get dictionary entry by int key
            var item = handler.Retrieve<Item>(cacheKey, 2);
            Assert.IsNotNull(item);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_TypedDictionary_StringKey_AddItem()
        {
            string cacheKey = "TestDictionary_TypedDictionary_StringKey_AddItem";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<string, Item>>(() => items.ToDictionary(x => x.ID.ToString())), TimeSpan.FromMinutes(30));

            var dict = handler.Retrieve<IDictionary<string, Item>>(cacheKey);
            Assert.IsNotNull(dict);

            handler.Replace(cacheKey, "20", new Item() { ID = 20, Count = 20, Name = "Twenty" });

            //See if we can get dictionary entry by int key
            var item = handler.Retrieve<Item>(cacheKey, "20");
            Assert.IsNotNull(item);
            Assert.AreEqual(20, item.ID);
            Assert.AreEqual("Twenty", item.Name);

            dict = handler.Retrieve<IDictionary<string, Item>>(cacheKey);
            Assert.IsNotNull(dict);
            Assert.AreEqual(11, dict.Count);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void TestDictionary_TypedDictionary_StringKey_Read()
        {
            string cacheKey = "TestDictionary_TypedDictionary_StringKey_Read";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<string, Item>>(() => items.ToDictionary(x => x.ID.ToString())), TimeSpan.FromMinutes(30));

            var dict = handler.Retrieve<IDictionary<string, Item>>(cacheKey);
            Assert.IsNotNull(dict);

            //See if we can get dictionary entry by int key
            var item = handler.Retrieve<Item>(cacheKey, "2");
            Assert.IsNotNull(item);

            handler.Remove(cacheKey);
        }

        private List<Item> GetNewItems(int count)
        {
            List<Item> items = new List<Item>();
            for (int i = 0; i < count; i++)
            {
                items.Add(new Item() { ID = i, Count = i * 100, Name = String.Format("Item{0}", i.ToString()) });
            }
            return items;
        }
    }

    public class Item
    {
        public int Count { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
    }

    //these two classes test the two cache base types we have. Each unit test in base class must pass
    //same set of conditions for both sub types thus ensuring they can be used interchangeably
    [TestClass]
    public class MemoryCacheTests : CacheTests
    {
        public MemoryCacheTests() : base(new MemoryCacheHandler())
        { }
    }

    [TestClass]
    public class RedisCacheTests : CacheTests
    {
        public RedisCacheTests() : base(null)
        {
            //Use connection info from CacheHandlerSection
            var handler = CacheHandlerSection.Instance.Handlers.First(x => x.Type.ToLower().Contains("redis")).Construct();
            base.handler = handler;
        }
    }
}
