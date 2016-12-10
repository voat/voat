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
using System.Collections.Generic;

using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Tests.Data
{
    public class CustomSearchOptions : SearchOptions
    {
        public Dictionary<string, string> CustomData = new Dictionary<string, string>();

        public CustomSearchOptions(string queryString) : base(queryString)
        {
        }

        protected override void ParseAdditionalKeyPairs(IEnumerable<KeyValuePair<string, string>> keypairs)
        {
            foreach (var kp in keypairs)
            {
                CustomData.Add(kp.Key, kp.Value);
            }
            base.ParseAdditionalKeyPairs(keypairs);
        }
    }

    [TestClass]
    public class SearchOptions_Tests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Search.Formatting"), TestCategory("Search.Parse")]
        public void SearchOptions_Count()
        {
            SearchOptions options = SearchOptions.Default;
            options.Count = 50;
            Assert.AreEqual(String.Format("count={0}", 50), options.ToString());
        }

        [TestMethod]
        [TestCategory("Search.Formatting"), TestCategory("Search.Parse")]
        public void SearchOptions_Default()
        {
            SearchOptions options = SearchOptions.Default;

            Assert.AreEqual("", options.ToString());
        }

        [TestMethod]
        [TestCategory("Search.Formatting"), TestCategory("Search.Parse")]
        public void SearchOptions_ExtendedObject()
        {
            CustomSearchOptions options = new CustomSearchOptions("help=true&count=50&index=2&mydata=some value");

            Assert.IsTrue(options.CustomData.ContainsKey("help"));
            Assert.IsTrue(options.CustomData.ContainsKey("mydata"));
            Assert.IsFalse(options.CustomData.ContainsKey("count")); //we now don't parse this value
            Assert.IsFalse(options.CustomData.ContainsKey("index")); //we now don't parse this value
        }

        [TestMethod]
        [TestCategory("Search.Formatting")]
        public void SearchOptions_ExtendedObject_ToString()
        {
            CustomSearchOptions options = new CustomSearchOptions("index=2&help=true&count=50&mydata=some value");

            ////Removed index based setup
            //Assert.AreEqual("count=50&index=2&help=true&mydata=some value", options.ToString());
            //Assert.AreEqual("count=50&help=true&index=2&mydata=some value", options.ToString());
            Assert.AreEqual("help=true&mydata=some value", options.ToString());
        }

        [TestMethod]
        [TestCategory("Search.Formatting"), TestCategory("Search.Parse")]
        public void SearchOptions_Page2()
        {
            SearchOptions options = SearchOptions.Default;
            options.Page = 2;
            options.Count = 50;
            Assert.AreEqual(String.Format("count={0}&page={2}", 50, 101, 2), options.ToString());
        }

        [TestMethod]
        [TestCategory("Search.Formatting"), TestCategory("Search.Parse")]
        public void SearchOptions_Parsing()
        {
            SearchOptions options = SearchOptions.Default;
            options.Page = 2;
            //options.Count = 50;
            options.Phrase = "hello";
            options.Sort = SortAlgorithm.Active;
            options.SortDirection = SortDirection.Reverse;
            options.StartDate = DateTime.UtcNow;

            var query = options.ToString();
            var newOptions = new SearchOptions(query);
            Assert.AreEqual(query, newOptions.ToString());
        }

        [TestMethod]
        [TestCategory("Search.Formatting"), TestCategory("Search.Parse")]
        public void SearchOptions_Search()
        {
            SearchOptions options = SearchOptions.Default;
            options.Page = 2;
            options.Count = 50;
            options.Phrase = "hello";
            Assert.AreEqual(String.Format("count={0}&page={1}&phrase={2}", 50, 2, "hello"), options.ToString());
        }
    }
}
