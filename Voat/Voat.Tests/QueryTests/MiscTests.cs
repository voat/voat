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
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache(null, new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, "news")));
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache(null, new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.DEFAULT)));
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache(null, new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.ANY)));
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache(null, new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.FRONT)));
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache(null, new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.ALL)));
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache(null, new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, "all")));
            //User
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache("UnitTest", new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, "news")));
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache("UnitTest", new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.DEFAULT)));
            Assert.AreEqual(false, QuerySubmissions.IsUserVolatileCache("UnitTest", new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.ANY)));
            Assert.AreEqual(true, QuerySubmissions.IsUserVolatileCache("UnitTest", new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.FRONT)));
            Assert.AreEqual(true, QuerySubmissions.IsUserVolatileCache("UnitTest", new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, Voat.Data.AGGREGATE_SUBVERSE.ALL)));
            Assert.AreEqual(true, QuerySubmissions.IsUserVolatileCache("UnitTest", new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, "all")));
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


        [TestMethod]
        public void GetFeatured_Test()
        {
            //Just makes sure query doesn't choke
            using (var repo = new Voat.Data.Repository())
            {
                var result = repo.GetFeatured();
            }
        }
    }
}
