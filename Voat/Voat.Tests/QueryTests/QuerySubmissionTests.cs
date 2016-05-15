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
using Voat.Caching;
using Voat.Data;
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
    public class QuerySubmissionTests 
    {
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Comment")]
        public void Query_Comment_Anon()
        {
            var q = new QueryComment(3, null);
            var result = q.Execute().Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ID.ToString(), result.UserName);
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Comment")]
        public void Query_CommentTree()
        {
            var q = new QueryCommentTree(1, null);
            var result = q.Execute().Result.Values;
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Count);

            foreach (var t in result)
            {
                Assert.AreEqual(t.IsAnonymized, t.UserName == t.ID.ToString());
            }
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Anon")]
        public void Query_CommentTree_Anon()
        {
            var q = new QueryCommentTree(2, null);
            var result = q.Execute().Result.Values;
            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Count);
            foreach (var t in result)
            {
                Assert.AreEqual(t.IsAnonymized, t.UserName == t.ID.ToString());
            }
        }

        /// <summary>
        /// This test ensures that if no user is currently logged in, we still return a default preference set.
        /// </summary>
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("User.Preferences")]
        public void Query_UserPreferences_Default()
        {
            var q = new QueryUserPreferences();
            var result = q.Execute().Result;
            Assert.IsNotNull(result);
            Assert.AreEqual("en", result.Language);

            q = new QueryUserPreferences();
            result = q.Execute().Result;
            Assert.IsNotNull(result);
            Assert.AreEqual("en", result.Language);
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        public void Query_v_All_Guest()
        {
            var q = new QuerySubmissions("_all", SearchOptions.Default, null);
            //q.CachePolicy.Duration = TimeSpan.Zero; //Turn off caching on this request
            var result = q.Execute().Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Cache")]
        public void Query_v_All_Guest_Cached()
        {
            var q = new QuerySubmissions("_all", SearchOptions.Default, new CachePolicy(TimeSpan.FromSeconds(30)));
            //q.CachePolicy.Duration = TimeSpan.FromSeconds(30); //Cache this request
            var result = q.Execute().Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(false, q.CacheHit);
            Assert.IsTrue(result.Any());

            q = new QuerySubmissions("_all", SearchOptions.Default, new CachePolicy(TimeSpan.FromMinutes(10)));
            result = q.Execute().Result;
            Assert.AreEqual(CacheHandler.Instance.CacheEnabled, q.CacheHit); //ensure second query hits cache
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Cache")]
        public void Query_v_All_Guest_Cached_Expired_Correctly()
        {
            TimeSpan cacheTime = TimeSpan.FromSeconds(5);

            var q = new QuerySubmissions("_all", new SearchOptions() { Count = 17 }, new CachePolicy(cacheTime));
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = q.Execute().Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(false, q.CacheHit);
            Assert.IsTrue(result.Any());

            //wait for cache to expire - Runtime caches aren't precise so wait long enough to ensure cached item is removed.
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(30));

            q = new QuerySubmissions("_all", new SearchOptions() { Count = 17 }, new CachePolicy(cacheTime));
            result = q.Execute().Result;
            //ensure we had to retreive new data
            Assert.AreEqual(false, q.CacheHit);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }
        
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Cache")]
        public void QuerySubverseInformation_verify_moderators_works()
        {
            TimeSpan cacheTime = TimeSpan.FromSeconds(5);

            var q = new QuerySubverseInformation("AuthorizedOnly");
            //q.CachePolicy.Duration = cacheTime; //Cache this request
            var result = q.Execute().Result;

            Assert.IsNotNull(result, "Expected result is null");
            Assert.IsNotNull(result.Moderators, "Expected Moderators property is null, was expecting list");

            Assert.IsTrue(result.Moderators.Any(x => x == "unit"));
        }

    }
}
