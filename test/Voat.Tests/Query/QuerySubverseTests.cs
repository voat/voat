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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.Infrastructure;

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
            var q = new QuerySubverseInformation(SUBVERSES.Disabled);
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
            var q = new QuerySubverseInformation(SUBVERSES.Anon);
            var r = q.ExecuteAsync().Result;
            Assert.IsNotNull(r, "Expecting a non-null return");
            Assert.AreEqual(true, r.IsAnonymized, "Expecting anonymized");
        }
    }
}
