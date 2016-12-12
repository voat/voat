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

using System;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Repository;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class BaseQueryMemoryCache : QuerySubmissionTests
    {
        public BaseQueryMemoryCache()
        {
            CacheHandler.Instance = new MemoryCacheHandler();
        }
    }

    [TestClass]
    public class BaseQueryNullCache : QuerySubmissionTests
    {
        public BaseQueryNullCache()
        {
            CacheHandler.Instance = new NullCacheHandler();
        }
    }

    [TestClass]
    public class BaseQueryRedisCache : QuerySubmissionTests
    {
        public BaseQueryRedisCache()
        {
            var handler = CacheHandlerSection.Instance.Handlers.FirstOrDefault(x => x.Type.ToLower().Contains("redis")).Construct();
            CacheHandler.Instance = handler;
        }
    }

    //[TestClass]
    public class QuerySubmissionTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Comment")]
        public void Query_Comment_Anon()
        {
            var q = new QueryComment(3, null);
            var result = q.ExecuteAsync().Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ID.ToString(), result.UserName);
        }

        //[TestMethod]
        //[TestCategory("Query")]
        //[TestCategory("Comment")]
        //public void Query_CommentTree()
        //{
        //    var q = new QueryCommentTree(1, null);
        //    var result = q.ExecuteAsync().Result.Values;
        //    Assert.IsNotNull(result);
        //    Assert.AreNotEqual(0, result.Count);

        //    foreach (var t in result)
        //    {
        //        Assert.AreEqual(t.IsAnonymized, t.UserName == t.ID.ToString());
        //    }
        //}

        //[TestMethod]
        //[TestCategory("Query")]
        //[TestCategory("Comment")]
        //[TestCategory("Comment.Anon")]
        //public void Query_CommentTree_Anon()
        //{
        //    var q = new QueryCommentTree(2, null);
        //    var result = q.ExecuteAsync().Result.Values;
        //    Assert.IsNotNull(result);
        //    Assert.AreNotEqual(0, result.Count);
        //    foreach (var t in result)
        //    {
        //        Assert.AreEqual(t.IsAnonymized, t.UserName == t.ID.ToString());
        //    }
        //}

        /// <summary>
        /// This test ensures that if no user is currently logged in, we still return a default preference set.
        /// </summary>
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("User.Preferences")]
        public async Task Query_UserPreferences_Default()
        {
            var q = new QueryUserPreferences();
            var result = await q.ExecuteAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual("en", result.Language);

            q = new QueryUserPreferences();
            result = await q.ExecuteAsync();
            Assert.IsNotNull(result, this.GetType().Name);
            Assert.AreEqual("en", result.Language, this.GetType().Name);
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        public void Query_v_All_Guest()
        {
            var q = new QuerySubmissions("_all", SearchOptions.Default, null);
            //q.CachePolicy.Duration = TimeSpan.Zero; //Turn off caching on this request
            var result = q.ExecuteAsync().Result;

            Assert.IsNotNull(result, this.GetType().Name);
            Assert.IsTrue(result.Any(), this.GetType().Name);
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Cache")]
        public void Query_v_All_Guest_Cached()
        {
            var q = new QuerySubmissions("_all", SearchOptions.Default, new CachePolicy(TimeSpan.FromSeconds(30)));
            //q.CachePolicy.Duration = TimeSpan.FromSeconds(30); //Cache this request
            var result = q.ExecuteAsync().Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(false, q.CacheHit);
            Assert.IsTrue(result.Any());

            q = new QuerySubmissions("_all", SearchOptions.Default, new CachePolicy(TimeSpan.FromMinutes(10)));
            result = q.ExecuteAsync().Result;
            Assert.AreEqual(CacheHandler.Instance.CacheEnabled, q.CacheHit, this.GetType().Name); //ensure second query hits cache
            Assert.IsNotNull(result, this.GetType().Name);
            Assert.IsTrue(result.Any(), this.GetType().Name);
        }

        //[Ignore] //Since CachedQuery.ExecuteAsync is not using the cachehandler.Register method this test will fail on MemoryCache that requires managed removal
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Cache")]
        public async Task Query_v_All_Guest_Cached_Expired_Correctly()
        {
            TimeSpan cacheTime = TimeSpan.FromSeconds(2);

            var q = new QuerySubmissions("_all", new SearchOptions() { Count = 17 }, new CachePolicy(cacheTime));
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = await q.ExecuteAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(false, q.CacheHit);
            Assert.IsTrue(result.Any());

            int waitTime = 0;
            //adjust sleep times based on cache type (trying to not have long running unit tests)
            if (this is BaseQueryRedisCache)
            {
                waitTime = 35;
            }
            else if (this is BaseQueryMemoryCache)
            {
                waitTime = 30;
            }

            //wait for cache to expire - Runtime caches aren't precise so wait long enough to ensure cached item is removed.
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(waitTime));

            q = new QuerySubmissions("_all", new SearchOptions() { Count = 17 }, new CachePolicy(cacheTime));
            result = await q.ExecuteAsync();
            //ensure we had to retreive new data
            Assert.AreEqual(false, q.CacheHit, this.GetType().Name);
            Assert.IsNotNull(result, this.GetType().Name);
            Assert.IsTrue(result.Any(), this.GetType().Name);
        }
        
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Subverse")]
        public void QuerySubverseInformation_verify_moderators_works()
        {
            TimeSpan cacheTime = TimeSpan.FromSeconds(5);

            var q = new QuerySubverseInformation("AuthorizedOnly");
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = q.ExecuteAsync().Result;

            Assert.IsNotNull(result, "Expected result is null");
            Assert.IsNotNull(result.Moderators, "Expected Moderators property is null, was expecting list");

            Assert.IsTrue(result.Moderators.Any(x => x == "unit"));
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        public async Task QuerySubmissions_Verify_BlockedSubverses()
        {

            //Ensure v/unit does not show up in v/all for user BlocksUnit
            TestHelper.SetPrincipal("BlocksUnit");

            var cmd = new BlockCommand(Domain.Models.DomainType.Subverse, "unit");
            var r = await cmd.Execute();

            var q = new QuerySubmissions("_all", SearchOptions.Default);
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = q.ExecuteAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(), "Found no results");

            foreach (var s in result)
            {
                Assert.AreNotEqual("unit", s.Subverse.ToLower(), "Found blocked sub in BlocksUnit's v/all query");
            }
        }

    }
}
