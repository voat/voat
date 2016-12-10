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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Domain.Models;

namespace Voat.Tests.Cache
{
    public abstract class CacheTests : BaseUnitTest
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

            Assert.AreEqual(handler.CacheEnabled, handler.Exists(key));
            if (handler.CacheEnabled)
            {
                var val = handler.Retrieve<Item>(key);
                Assert.IsNotNull(val);
                Assert.AreEqual(1, val.ID);
                Assert.AreEqual("Bill", val.Name);
            }

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
            if (handler.CacheEnabled)
            {
                Assert.AreEqual(true, handler.Exists(key));
                var val = handler.Retrieve<long>(key);
                Assert.AreEqual(1, val);
            }
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
        public void Dictionary_Add()
        {
            string cacheKey = "Dictionary_Add";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));

            if (handler.CacheEnabled)
            {
                //handler.RegisterDictionary(cacheKey, new Func<IList<Item>>(() => items), new Func<Item, object>((x) => x.ID), TimeSpan.FromMinutes(100));
                var r = handler.Retrieve<IDictionary>(cacheKey);

                Assert.IsNotNull(r);
                Assert.AreEqual(items.Count, r.Count);

                var item = handler.DictionaryRetrieve<int, Item>(cacheKey, 1);
                Assert.IsNotNull(item);
                Assert.AreEqual(1, item.ID);

            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_RemoveExisting()
        {
            string cacheKey = "Dictionary_RemoveExisting";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));

            //replace existing key
            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.DictionaryRemove(cacheKey, 1);

            var cachedItem = handler.DictionaryRetrieve<int, Item>(cacheKey, 1);

            Assert.IsNull(cachedItem);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_RemoveNonExisting()
        {
            string cacheKey = "Dictionary_RemoveNonExisting";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));

            //replace existing key
            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.DictionaryRemove(cacheKey, 20);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_ReplaceExisting()
        {
            string cacheKey = "Dictionary_ReplaceExisting";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<object, Item>>(() => items.ToDictionary(new Func<Item, object>(x => x.ID))), TimeSpan.FromMinutes(30));

            //replace existing key
            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.DictionaryReplace(cacheKey, 1, newItem);
            if (handler.CacheEnabled)
            {
                var cachedItem = handler.DictionaryRetrieve<int, Item>(cacheKey, 1);

                Assert.IsNotNull(cachedItem);
                Assert.AreEqual(newItem.ID, cachedItem.ID);
                Assert.AreEqual(newItem.Name, cachedItem.Name);
                Assert.AreEqual(newItem.Count, cachedItem.Count);
            }
            else
            {
                var cachedItem = handler.DictionaryRetrieve<int, Item>(cacheKey, 1);
                Assert.IsNull(cachedItem);
            }

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_ReplaceWithoutPrevious_1()
        {
            //we used to add this to cache if it didn't exist, now we don't
            string cacheKey = "Dictionary_Add";
            handler.Remove(cacheKey);

            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.DictionaryReplace<int, Item>(cacheKey, 1, newItem);
            Assert.IsFalse(handler.DictionaryExists(cacheKey, 1));

            var newItem2 = new Item() { ID = 2, Count = 400, Name = "New Name" };
            handler.DictionaryReplace(cacheKey, 2, newItem2);
            Assert.IsFalse(handler.DictionaryExists(cacheKey, 2));

            var cacheItem = handler.DictionaryRetrieve<int, Item>(cacheKey, 1);
            Assert.IsNull(cacheItem);
            //Assert.AreEqual(newItem.ID, cacheItem.ID);
            //Assert.AreEqual(newItem.Count, cacheItem.Count);
            //Assert.AreEqual(newItem.Name, cacheItem.Name);

            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_ReplaceWithoutPrevious_2()
        {
            //we used to add this to cache if it didn't exist, now we don't
            string cacheKey = "Dictionary_Replace_Callback";
            handler.Remove(cacheKey);

            var newItem = new Item() { ID = 1, Count = 400, Name = "New Name" };
            handler.Register(cacheKey, () => new Dictionary<int, Item>() { { 1, newItem } }, TimeSpan.Zero);

            if (handler.CacheEnabled)
            {
                handler.DictionaryReplace<int, Item>(cacheKey, 1, (x) => { Assert.IsNotNull(x, "Condition 1"); return x; }, true);
                handler.DictionaryReplace<int, Item>(cacheKey, 1, (x) => { Assert.IsNotNull(x, "Condition 2"); return x; }, false);

                handler.DictionaryReplace<int, Item>(cacheKey, 2, (x) => { Assert.IsNull(x, "Condition 3"); return x; }, false);
                handler.DictionaryReplace<int, Item>(cacheKey, 2, (x) => { Assert.Fail("Condition 4"); return x; }, true);
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_TypedDictionary_EnumKey_AddItem()
        {
            string cacheKey = "Dictionary_TypedDictionary_EnumKey_AddItem";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<DomainType, IList<Item>>>(() =>
            {
                var dictionary = new Dictionary<DomainType, IList<Item>> { { DomainType.Subverse, items }, { DomainType.Set, items }, { DomainType.User, new List<Item>() } };
                return dictionary;
            }), TimeSpan.FromMinutes(30));

            if (handler.CacheEnabled)
            {
                var dict = handler.Retrieve<IDictionary<DomainType, IList<Item>>>(cacheKey);
                Assert.IsNotNull(dict);

                handler.DictionaryReplace(cacheKey, DomainType.User, new List<Item>() { new Item() { ID = 20, Count = 20, Name = "Twenty" } });

                //See if we can get updated dictionary entry by enum key
                var data = handler.DictionaryRetrieve<DomainType, IList<Item>>(cacheKey, DomainType.User);
                var item = data.First();
                Assert.IsNotNull(item);
                Assert.AreEqual(20, item.ID);
                Assert.AreEqual("Twenty", item.Name);

                dict = handler.Retrieve<IDictionary<DomainType, IList<Item>>>(cacheKey);
                Assert.IsNotNull(dict);
                Assert.AreEqual(3, dict.Count);
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_TypedDictionary_IntKey_AddItem()
        {
            string cacheKey = "Dictionary_TypedDictionary_IntKey_AddItem";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<int, Item>>(() => items.ToDictionary(x => x.ID)), TimeSpan.FromMinutes(30));
            if (handler.CacheEnabled)
            {

                var dict = handler.Retrieve<IDictionary<int, Item>>(cacheKey);
                Assert.IsNotNull(dict);

                handler.DictionaryReplace(cacheKey, 20, new Item() { ID = 20, Count = 20, Name = "Twenty" });

                //See if we can get dictionary entry by int key
                var item = handler.DictionaryRetrieve<int, Item>(cacheKey, 20);
                Assert.IsNotNull(item);
                Assert.AreEqual(20, item.ID);
                Assert.AreEqual("Twenty", item.Name);

                dict = handler.Retrieve<IDictionary<int, Item>>(cacheKey);
                Assert.IsNotNull(dict);
                Assert.AreEqual(11, dict.Count);
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_TypedDictionary_IntKey_Read()
        {
            string cacheKey = "Dictionary_TypedDictionary_IntKey_Read";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<int, Item>>(() => items.ToDictionary(x => x.ID)), TimeSpan.FromMinutes(30));

            if (handler.CacheEnabled)
            {
                var dict = handler.Retrieve<IDictionary<int, Item>>(cacheKey);
                Assert.IsNotNull(dict);

                //See if we can get dictionary entry by int key
                var item = handler.DictionaryRetrieve<int, Item>(cacheKey, 2);
                Assert.IsNotNull(item);
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_TypedDictionary_StringKey_AddItem()
        {
            string cacheKey = "Dictionary_TypedDictionary_StringKey_AddItem";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<string, Item>>(() => items.ToDictionary(x => x.ID.ToString())), TimeSpan.FromMinutes(30));

            if (handler.CacheEnabled)
            {
                var dict = handler.Retrieve<IDictionary<string, Item>>(cacheKey);
                Assert.IsNotNull(dict);

                handler.DictionaryReplace(cacheKey, "20", new Item() { ID = 20, Count = 20, Name = "Twenty" });

                //See if we can get dictionary entry by int key
                var item = handler.DictionaryRetrieve<string, Item>(cacheKey, "20");
                Assert.IsNotNull(item);
                Assert.AreEqual(20, item.ID);
                Assert.AreEqual("Twenty", item.Name);

                dict = handler.Retrieve<IDictionary<string, Item>>(cacheKey);
                Assert.IsNotNull(dict);
                Assert.AreEqual(11, dict.Count);
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Dictionary")]
        public void Dictionary_TypedDictionary_StringKey_Read()
        {
            string cacheKey = "Dictionary_TypedDictionary_StringKey_Read";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            handler.Register(cacheKey, new Func<IDictionary<string, Item>>(() => items.ToDictionary(x => x.ID.ToString())), TimeSpan.FromMinutes(30));

            if (handler.CacheEnabled)
            {
                var dict = handler.Retrieve<IDictionary<string, Item>>(cacheKey);
                Assert.IsNotNull(dict);

                //See if we can get dictionary entry by int key
                var item = handler.DictionaryRetrieve<string, Item>(cacheKey, "2");
                Assert.IsNotNull(item);
            }
            handler.Remove(cacheKey);
        }
        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        public void ReplaceIfExistsTests()
        {
            string cacheKey = "ReplaceIfExistsTests";
            handler.Remove(cacheKey);

            //No entry tests
            string testValue = "TestValue";
            bool funcCalled = false;
            handler.ReplaceIfExists<string>(cacheKey, x => { funcCalled = true; return testValue.ToUpper(); } );

            Assert.AreEqual(false, handler.Exists(cacheKey));
            Assert.AreEqual(false, funcCalled);

            handler.ReplaceIfExists(cacheKey, testValue);
            Assert.AreEqual(false, handler.Exists(cacheKey));

            //This won't pass with NullCacheHandler
            if (handler.CacheEnabled)
            {
                //Existing Entry tests
                handler.Replace(cacheKey, testValue);
                Assert.AreEqual(true, handler.Exists(cacheKey));

                handler.ReplaceIfExists(cacheKey, testValue.ToUpper());
                Assert.AreEqual(true, handler.Exists(cacheKey));
                Assert.AreEqual(testValue.ToUpper(), handler.Retrieve<string>(cacheKey));

                handler.ReplaceIfExists<string>(cacheKey, x => { funcCalled = true; return $"{testValue}{testValue}".ToUpper(); });
                Assert.AreEqual(true, funcCalled);
                Assert.AreEqual($"{testValue}{testValue}".ToUpper(), handler.Retrieve<string>(cacheKey));
            }


            handler.Remove(cacheKey);
        }
        #region Set Operations

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.List")]
        public void Set_AddToExisting()
        {
            string cacheKey = "Set_AddToExisting";
            handler.Remove(cacheKey);

            List<int> items = new List<int>() { 1, 2, 3, 4, 5 };
            HashSet<int> set = new HashSet<int>(items);

            handler.Register(cacheKey, new Func<ISet<int>>(() => set), TimeSpan.FromMinutes(30));

            handler.SetAdd(cacheKey, 6);
            if (handler.CacheEnabled)
            {
                var exists = handler.SetExists(cacheKey, 6);
                Assert.AreEqual(true, exists);
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.List")]
        public void Set_Add_Ints()
        {
            string cacheKey = "Set_Add_Ints";
            handler.Remove(cacheKey);

            List<int> items = new List<int>() { 1, 2, 3, 4, 5 };
            HashSet<int> set = new HashSet<int>(items);

            handler.Register(cacheKey, new Func<ISet<int>>(() => set), TimeSpan.FromMinutes(30));
            if (handler.CacheEnabled)
            {
                foreach (var item in items)
                {
                    var exists = handler.SetExists(cacheKey, item);
                    Assert.AreEqual(true, exists);
                }
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.List")]
        public void Set_Add_Object()
        {
            string cacheKey = "Set_Add";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            HashSet<Item> set = new HashSet<Item>(items);
            handler.Register(cacheKey, new Func<ISet<Item>>(() => set), TimeSpan.FromMinutes(30));
            if (handler.CacheEnabled)
            {
                foreach (var item in items)
                {
                    var exists = handler.SetExists(cacheKey, item);
                    Assert.AreEqual(true, exists);
                }
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.List")]
        public void Set_Remove()
        {
            string cacheKey = "Set_Remove";
            handler.Remove(cacheKey);

            List<Item> items = GetNewItems(10);
            HashSet<Item> set = new HashSet<Item>(items);
            handler.Register(cacheKey, new Func<ISet<Item>>(() => set), TimeSpan.FromMinutes(30));

            if (handler.CacheEnabled)
            {
                foreach (var item in items)
                {
                    var exists = handler.SetExists(cacheKey, item);
                    Assert.AreEqual(true, exists);
                }
            }
            foreach (var item in items)
            {
                handler.SetRemove(cacheKey, item);
            }

            foreach (var item in items)
            {
                var exists = handler.SetExists(cacheKey, item);
                Assert.AreEqual(false, exists);
            }
            handler.Remove(cacheKey);
        }

        [TestMethod]
        [TestCategory("Cache")]
        [TestCategory("Cache.Handler")]
        [TestCategory("Cache.Handler.Refresh")]
        public async Task Hot_Cache_Refresh()
        {
            string cacheKey = "Hot_Cache_Refresh";
            handler.Remove(cacheKey);

            int? count = 0;
            int? originalCount = count;

            if (handler.RefetchEnabled)
            {
                handler.Register(cacheKey, () => {

                    count = (count + 1);
                    return count;

                }, TimeSpan.FromSeconds(15), 5);



                if (handler.CacheEnabled)
                {
                    //Try to bail as quickly as possible once we know the hot cache is functioning
                    var startTime = DateTime.Now;
                    var timeoutTimeSpan = TimeSpan.FromSeconds(60);
                    var hasUpdated = false;
                    while (!hasUpdated && DateTime.Now.Subtract(startTime) < timeoutTimeSpan)
                    {
                        int? checkValue = handler.Retrieve<int?>(cacheKey);
                        hasUpdated = (checkValue.HasValue && checkValue.Value != 0 && checkValue.Value != 1);
                        if (!hasUpdated)
                        {
                            await Task.Delay(5000);
                        }
                    }

                    int? newCount = handler.Retrieve<int?>(cacheKey);
                    Assert.IsNotNull(newCount); //Make sure we still have it
                    Assert.AreNotEqual(0, newCount); //Make sure default value is not inserted
                    Assert.AreNotEqual(1, newCount); //This will be the first value inserted
                }
                handler.Remove(cacheKey);
            }
            else
            {
                Assert.Inconclusive("Cache Refetch not enabled");
            }
        }

        #endregion Set Operations

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
        {
            Debug.Print("Starting MemoryCacheTests");
        }
    }

    [TestClass]
    public class RedisCacheTests : CacheTests
    {
        //Stop following me fuzzy
        public RedisCacheTests() : base(null)
        {
            Debug.Print("Starting RedisCacheTests");

            //Use connection info from CacheHandlerSection
            var handler = CacheHandlerSection.Instance.Handlers.First(x => x.Type.ToLower().Contains("redis")).Construct();
            base.handler = handler;

        }
    }
    [TestClass]
    public class NullCacheTests : CacheTests
    {
        public NullCacheTests() : base(new NullCacheHandler())
        {
            Debug.Print("Starting NullCacheTests");
        }
    }
}
