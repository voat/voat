using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class BatchOperationTests : BaseUnitTest
    {
        public class BatchItem
        {
            public string ID { get; } = Guid.NewGuid().ToString();
        }

        [TestMethod]
        [TestCategory("Batch")]
        public void CacheBatchTests()
        {
            int count = 2000;
            var processed = false;

            var cacheHandler = Caching.CacheConfigurationSettings.Instance.Handler.Construct<ICacheHandler>();
            var batchItems = new List<BatchItem>();
            for (int i = 0; i < count; i++)
            {
                batchItems.Add(new BatchItem());
            }
            var c = new CacheBatchOperation<BatchItem>("CacheBatchTests", cacheHandler, 2000, TimeSpan.Zero, 
                new Func<IEnumerable<BatchItem>, Task>(x => {
                    processed = true;
                    return Task.CompletedTask;
                }));
            c.ClearPrevious = false;

            batchItems.ToList().ForEach(x => c.Add(x));

            System.Threading.Thread.Sleep(500);
            Assert.IsTrue(processed, "Batch did not fire");
        }

    }
}
