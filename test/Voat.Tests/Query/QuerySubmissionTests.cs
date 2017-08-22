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
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Repository;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QuerySubmissionTests : BaseUnitTest
    {

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        public async Task QuerySubmissions_By_Domain()
        {
            var domain = "LearnToGolfLikeCharlesBarkleyOrYourMoneyBack.com";

            var user = TestHelper.SetPrincipal("UnitTestUser48");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Title = "Best Offer Ever!", Url = $"https://www.{domain}/limited-time-offer-{Guid.NewGuid().ToString()}.html", Subverse = SUBVERSES.Unit }).SetUserContext(user);
            var r = await cmd.Execute();
            Assert.AreEqual(Status.Success, r.Status, r.Message);


            var q = new QuerySubmissionsByDomain(domain, SearchOptions.Default);
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = q.ExecuteAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(), "Found no results");

            foreach (var s in result)
            {
                Assert.IsTrue(s.Url.ToLower().Contains(domain.ToLower()), $"Couldn't find {domain} in {s.Url}");
            }
        }


        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        public async Task QuerySubmissions_Default_New_NotLogged_In()
        {
            var options = SearchOptions.Default;
            options.Sort = Domain.Models.SortAlgorithm.New;

            var q = new QuerySubmissions(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, AGGREGATE_SUBVERSE.DEFAULT), options);
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = await q.ExecuteAsync();

            //VoatAssert.IsValid(result);

            //Assert.IsTrue(result.Any(), "Found no results");

        }
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        public async Task QuerySubmissions_Front_New_NotLogged_In()
        {
            var options = SearchOptions.Default;
            options.Sort = Domain.Models.SortAlgorithm.New;

            var q = new QuerySubmissions(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, AGGREGATE_SUBVERSE.FRONT), options);
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = await q.ExecuteAsync();

            //VoatAssert.IsValid(result);

            //Assert.IsTrue(result.Any(), "Found no results");

        }
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        public async Task QuerySubmissions_All_New_NotLogged_In()
        {
            var options = SearchOptions.Default;
            options.Sort = Domain.Models.SortAlgorithm.New;

            var q = new QuerySubmissions(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, AGGREGATE_SUBVERSE.ALL), options);
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = await q.ExecuteAsync();

            //VoatAssert.IsValid(result);

            //Assert.IsTrue(result.Any(), "Found no results");

        }
    }
}
