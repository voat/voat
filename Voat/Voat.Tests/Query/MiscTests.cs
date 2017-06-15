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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Domain.Query;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class MiscTests : BaseUnitTest
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
        [TestCategory("Query")]
        public void GetFeatured_Test()
        {
            //Just makes sure query doesn't choke
            using (var repo = new Voat.Data.Repository())
            {
                var result = repo.GetFeatured();
            }
        }

        [TestMethod]
        [TestCategory("Query")]
        public async Task GetRandomSubverse_Test()
        {
            //Right now we just want to make sure no error occurs 

            var q = new QueryRandomSubverse(false);
            var randomSubverse = await q.ExecuteAsync();
            //Assert.IsNotNull(randomSubverse);

            q = new QueryRandomSubverse(true);
            randomSubverse = await q.ExecuteAsync();
            //Assert.IsNotNull(randomSubverse);

        }

    }
}
