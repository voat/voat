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

using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Voat.Caching;
using Voat.Configuration;

namespace Voat.Tests.Cache
{
    [TestClass]
    public class MiscTests : BaseUnitTest
    {
        public object ConnectionMultiplexer { get; private set; }

        public T Convert<T>(HashEntry[] state) where T : new()
        {
            T i = new T();
            Type t = typeof(T);
            state.ToList().ForEach(h =>
            {
                var prop = t.GetProperty(h.Name);
                prop.SetValue(i, System.Convert.ChangeType(h.Value, prop.PropertyType));
            });
            return i;
        }

        public HashEntry[] Convert<T>(T state) where T : new()
        {
            List<HashEntry> entries = new List<HashEntry>();
            Type t = typeof(T);
            t.GetProperties().ToList().ForEach(prop =>
            {
                entries.Add(new HashEntry(prop.Name, (dynamic)prop.GetValue(state)));
            });
            return entries.ToArray();
        }

        [TestMethod]
        public void TestDictionary()
        {
            var args = ArgumentParser.Parse( CacheHandlerSection.Instance.Handlers.FirstOrDefault(x => x.Type.ToLower().Contains("redis")).Arguments);

            var conn = StackExchange.Redis.ConnectionMultiplexer.Connect(args[0].ToString());
            var db = conn.GetDatabase(0);

            db.KeyDelete("Test");
            db.KeyDelete("Test2");
            db.KeyDelete("Dictionary:SeTest");
            db.KeyDelete("List:SeTest");
            db.KeyDelete("Set:SeTest");

            Item item = null; // new Item() { ID = 1, Name = "Bill" };
            item = new Item() { ID = 1, Name = "Bill" };
            db.HashSet("Test", new HashEntry[] { new HashEntry(item.ID, JsonConvert.SerializeObject(item)) });
            item = new Item() { ID = 2, Name = "Two" };
            db.HashSet("Test", "2", JsonConvert.SerializeObject(item));
            db.HashSet("Test2", Convert(item));

            var hg = db.HashGetAll("Test");
            var dict1 = hg.ToDictionary(x => int.Parse(x.Name), y => JsonConvert.DeserializeObject<Item>(y.Value));
            var dict = new Dictionary<int, Item>();

            //var dict = hg.ToDictionary<HashEntry, int>(x => int.Parse(x.Name));
            db.HashIncrement("Test2", "Count");
            db.HashIncrement("Test2", "Count");
            db.HashIncrement("Test2", "Count");
            db.HashIncrement("Test2", "Count");
            db.HashIncrement("Test2", "Count");

            //RedisValue
            var hashresults = db.HashGetAll("Test2");
            //StackExchange.Redis.ExtensionMethods.ToDictionary()

            var newItem = Convert<Item>(hashresults);

            var d = new StackExchange.Redis.DataTypes.RedisTypeFactory(conn).GetDictionary<int, Item>("SeTest");
            //var d = new StackExchange.Redis.DataTypes.Collections.RedisDictionary<int, Item>(db, "SeTest");
            item = new Item() { ID = 1, Name = "Bill" };
            d.Add(item.ID, item);
            item = new Item() { ID = 2, Name = "Two" };
            d.Add(item.ID, item);

            var s = new StackExchange.Redis.DataTypes.RedisTypeFactory(conn).GetSet<Item>("SeTest");
            //var d = new StackExchange.Redis.DataTypes.Collections.RedisDictionary<int, Item>(db, "SeTest");
            item = new Item() { ID = 1, Name = "Bill" };
            s.Add(item);
            item = new Item() { ID = 2, Name = "Two" };
            s.Add(item);

            var l = new StackExchange.Redis.DataTypes.RedisTypeFactory(conn).GetList<Item>("SeTest");
            //var d = new StackExchange.Redis.DataTypes.Collections.RedisDictionary<int, Item>(db, "SeTest");
            item = new Item() { ID = 1, Name = "Bill" };
            l.Add(item);
            item = new Item() { ID = 2, Name = "Two" };
            l.Add(item);

            //var redis = new RedisCacheHandler();
            //string key = DateTime.UtcNow.ToOADate().ToString();

            //handler.Register<Item>(key, () => , TimeSpan.FromSeconds(30));

            //Assert.AreEqual(true, handler.Exists(key));
            //var val = handler.Retrieve<Item>(key);
            //Assert.IsNotNull(val);
            //Assert.AreEqual(1, val.ID);
            //Assert.AreEqual("Bill", val.Name);
            //handler.Remove(key);
            //Assert.AreEqual(false, handler.Exists(key));
        }

        //[TestMethod]
        public void TestSerializationWithRedis()
        {
            var args = CacheHandlerSection.Instance.Handlers.FirstOrDefault(x => x.Type.ToLower().Contains("redis")).Arguments;

            //These passes are intended to ballpark how diff .NET to redis serialization stacks up.
            var conn = StackExchange.Redis.ConnectionMultiplexer.Connect(args);
            var db = conn.GetDatabase();

            List<KeyValuePair<long, string>> messages = new List<KeyValuePair<long, string>>();

            int iterations = 1000;
            int loops = 15;
            Stopwatch timer = null;
            Item final = null;
            Item item = null;
            for (int loop = 0; loop < loops; loop++)
            {
                ///////////////////////////////
                timer = new Stopwatch();
                timer.Start();
                item = new Item() { ID = 1, Name = "TimerTest", Count = 15 };
                db.HashSet("Test-Hash-Read", Convert(item));

                for (int i = 0; i < iterations; i++)
                {
                    db.HashIncrement("Test-Hash2", "Count");
                    item = Convert<Item>(db.HashGetAll("Test-Hash-Read"));
                }
                final = Convert<Item>(db.HashGetAll("Test-Hash-Read"));
                timer.Stop();
                messages.Add(new KeyValuePair<long, string>(timer.ElapsedMilliseconds, "Test-Hash-Read (Hash Increment w/Read)"));

                ///////////////////////////////
                timer = new Stopwatch();
                timer.Start();
                item = new Item() { ID = 1, Name = "TimerTest", Count = 15 };
                db.HashSet("Test-Hash2", Convert(item));

                for (int i = 0; i < iterations; i++)
                {
                    db.HashIncrement("Test-Hash2", "Count");
                }
                final = Convert<Item>(db.HashGetAll("Test-Hash"));
                timer.Stop();
                messages.Add(new KeyValuePair<long, string>(timer.ElapsedMilliseconds, "Test-Hash2 (Hash Increment)"));

                ///////////////////////////////
                timer = new Stopwatch();
                timer.Start();
                item = new Item() { ID = 1, Name = "TimerTest", Count = 15 };
                db.StringSet("Test-Json", JsonConvert.SerializeObject(item));

                for (int i = 0; i < iterations; i++)
                {
                    var x = JsonConvert.DeserializeObject<Item>(db.StringGet("Test-Json"));
                    x.Count = x.Count + 1;
                    db.StringSet("Test-Json", JsonConvert.SerializeObject(x));
                }
                final = JsonConvert.DeserializeObject<Item>(db.StringGet("Test-Json"));
                timer.Stop();
                messages.Add(new KeyValuePair<long, string>(timer.ElapsedMilliseconds, "Test-Json (Full Deserialization)"));

                ///////////////////////////////
                timer = new Stopwatch();
                timer.Start();
                item = new Item() { ID = 1, Name = "TimerTest", Count = 15 };
                db.HashSet("Test-Hash", Convert(item));

                for (int i = 0; i < iterations; i++)
                {
                    var x = Convert<Item>(db.HashGetAll("Test-Hash"));
                    x.Count = x.Count + 1;
                    db.HashSet("Test-Hash", Convert(x));
                }
                final = Convert<Item>(db.HashGetAll("Test-Hash"));
                timer.Stop();
                messages.Add(new KeyValuePair<long, string>(timer.ElapsedMilliseconds, "Test-Hash (Full Deserialization)"));
            }

            String message = "\nTotals (Highest Pass per Type ignored)\n\n";
            //messages.GroupBy(x => x.Value).Select(y => new { key = y.Key, total = y.OrderByDescending(x => x.Key).Skip(1).Sum(w => w.Key) }).OrderBy(x => x.total).ToList().ForEach(r => message += String.Format("{0}: {1}\n", r.key, r.total)); ;
            message = messages.GroupBy(x => x.Value).Select(y => new { key = y.Key, total = y.OrderByDescending(x => x.Key).Skip(1).Sum(w => w.Key) }).OrderBy(x => x.total)
                .Aggregate(message, (m, r) => m += String.Format("{0}: {1}\n", r.key, r.total));
            ;

            message += "\nPasses\n";
            messages.OrderBy(x => x.Key).ToList().ForEach(kp => message += String.Format("{1}: {0}\n", kp.Key, kp.Value));

            Assert.Inconclusive(message);
        }
    }
}
