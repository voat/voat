using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Cache
{
    [TestClass]
    public class MonitoredMemoryCacheTests : BaseUnitTest
    {
        public class CacheItem
        {
            public string Key { get; set; }
            public DateTime CreationDate { get; } = DateTime.UtcNow;

        }
        //[TestMethod]
        public async Task TestExcessiveRefresh()
        {
            //var memCache = new MemoryCacheHandler(true);
            var memCache = new RedisCacheHandler("127.0.1:6379,defaultDatabase=7,allowAdmin=true,syncTimeout=4000", true);

            var key = "TestKey";

            var getData = new Func<Task<CacheItem>>(async () => {

                Debug.WriteLine("In getData");

                await Task.Delay(2000);

                return await Task.FromResult(new CacheItem() { Key = key });
            });

            Task.WaitAll(new[] {
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10)
            });

            await Task.Delay(TimeSpan.FromSeconds(30));
            Task.WaitAll(new[] {
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                Task.Run(() => memCache.Remove(key)),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                Task.Run(() => memCache.Remove(key)),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10)
            });

            await Task.Delay(TimeSpan.FromSeconds(30));
            Task.WaitAll(new[] {
                Task.Run(() => memCache.Replace(key, getData())),
                Task.Run(() => memCache.Remove(key)),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                Task.Run(() => memCache.Remove(key)),
                memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10),
                Task.Run(() => memCache.Replace(key, getData())),
            });

            await memCache.Register(key, getData, TimeSpan.FromSeconds(10), 10);
            await Task.Delay(TimeSpan.FromSeconds(30));
            await Task.Delay(TimeSpan.FromSeconds(30));
            await Task.Delay(TimeSpan.FromSeconds(30));
            await Task.Delay(TimeSpan.FromSeconds(30));
            await Task.Delay(TimeSpan.FromSeconds(30));
            await Task.Delay(TimeSpan.FromSeconds(30));




        }
    }
}
