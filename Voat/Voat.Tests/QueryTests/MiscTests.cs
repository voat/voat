using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Domain.Query;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        public void TestSubmissionCacheVolatileLogic()
        {
            //No user
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache(null, "news"));
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache(null, Voat.Data.AGGREGATE_SUBVERSE.DEFAULT));
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache(null, Voat.Data.AGGREGATE_SUBVERSE.ANY));
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache(null, Voat.Data.AGGREGATE_SUBVERSE.FRONT));
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache(null, Voat.Data.AGGREGATE_SUBVERSE.ALL));
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache(null, "all"));
            //User
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache("UnitTest", "news"));
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache("UnitTest", Voat.Data.AGGREGATE_SUBVERSE.DEFAULT));
            Assert.AreEqual(false, QuerySubmissionsLegacy.IsUserVolatileCache("UnitTest", Voat.Data.AGGREGATE_SUBVERSE.ANY));
            Assert.AreEqual(true, QuerySubmissionsLegacy.IsUserVolatileCache("UnitTest", Voat.Data.AGGREGATE_SUBVERSE.FRONT));
            Assert.AreEqual(true, QuerySubmissionsLegacy.IsUserVolatileCache("UnitTest", Voat.Data.AGGREGATE_SUBVERSE.ALL));
            Assert.AreEqual(true, QuerySubmissionsLegacy.IsUserVolatileCache("UnitTest", "all"));
        }

        [TestMethod]
        public void TestCachePolicyComparison()
        {
            Assert.AreEqual(false, (new CachePolicy(TimeSpan.Zero) == null));
            Assert.AreEqual(false, (new CachePolicy(TimeSpan.Zero, 2) == CachePolicy.None));
            Assert.AreEqual(false, (new CachePolicy(TimeSpan.Zero, 2) == new CachePolicy(TimeSpan.Zero, 3)));


            Assert.AreEqual(true, (new CachePolicy(TimeSpan.Zero) == CachePolicy.None));
            Assert.AreEqual(true, (new CachePolicy(TimeSpan.FromMinutes(5)) == new CachePolicy(TimeSpan.FromMinutes(5), -1)));

            Assert.AreEqual(true, (new CachePolicy(TimeSpan.FromMinutes(5), 10) == new CachePolicy(TimeSpan.FromMinutes(5), 10)));
            var policy = new CachePolicy(TimeSpan.FromMinutes(15), 4);
            Assert.AreEqual(true, (policy == policy));

        }
    }
}
