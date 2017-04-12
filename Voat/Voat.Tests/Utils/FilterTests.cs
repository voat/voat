using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data.Models;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class FilterTests : BaseUnitTest
    {
        public override void ClassInitialize()
        {
            using (var db = new voatEntities())
            {
                //1
                db.Filters.Add(new Filter() {
                    Pattern = ".",
                    IsActive = false,
                    Name = "Match Anything",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                });
                //2
                db.Filters.Add(new Filter()
                {
                    Pattern = @"evildomain\.evil",
                    IsActive = true,
                    Name = "Evil Domain is Banned",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                });
                //3
                db.Filters.Add(new Filter()
                {
                    Pattern = @"google\.com/url",
                    IsActive = true,
                    Name = "Google redirect ban",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                });
                db.SaveChanges();
            }
            //Remove Filters that may be in cache
            CacheHandler.Instance.Remove(CachingKey.Filters());
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.Filters")]
        public void TestBadData()
        {
            IEnumerable<FilterMatch> result = null;

            result = FilterUtility.Match("");
            Assert.IsFalse(result.Any());

            result = FilterUtility.Match(null);
            Assert.IsFalse(result.Any());

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.Filters")]
        public void TestBasicFilterMatch()
        {
            IEnumerable<FilterMatch> result = null;

            result = FilterUtility.Match("Test Data");
            Assert.IsFalse(result.Any());

            result = FilterUtility.Match("I have this new site, check it: evildomain.evil. Tell me if you like it");
            Assert.IsTrue(result.Any());
            Assert.AreEqual(2, result.First().Filter.ID);

            //Test casing
            result = FilterUtility.Match("I have this new site, check it: EVilDOmAiN.evIL. Tell me if you like it");
            Assert.IsTrue(result.Any());
            Assert.AreEqual(2, result.First().Filter.ID);

            var multipleViolations = "Check out my super awesome content [https://www.google.com/some-made-up-markdown-url/](https://www.google.com/url?q=http%3A%2F%2Fwww.evildomain.evil%2F%3Ftrack%3Dvoat%26keyword%3Dsome%2Bkey%2Bword%26charset%3Dutf-8). Do you like?";

            //should return only first match
            result = FilterUtility.Match(multipleViolations);
            Assert.IsTrue(result.Any());
            Assert.AreEqual(3, result.First().Filter.ID);

            //see if match all returns both violations
            result = FilterUtility.Match(multipleViolations, true);
            Assert.AreEqual(2, result.Count());

        }
    }
}
