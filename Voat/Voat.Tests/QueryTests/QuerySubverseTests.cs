using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QuerySubverseTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Query.Subverse")]
        public void QueryDisabledSubverse()
        {
            var q = new QuerySubverseInformation("Disabled");
            var r = q.ExecuteAsync().Result;
            Assert.IsNull(r, "Expecting disabled sub info to return null");
        }
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Query.Subverse")]
        [TestCategory("Anon")]
        public void QuerySubverse_Anon()
        {
            var q = new QuerySubverseInformation("anon");
            var r = q.ExecuteAsync().Result;
            Assert.IsNotNull(r, "Expecting a non-null return");
            Assert.AreEqual(true, r.IsAnonymized, "Expecting anonymized");
        }
    }
}
