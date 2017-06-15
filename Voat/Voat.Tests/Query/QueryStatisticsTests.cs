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

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Domain.Command;
using Voat.Utilities;
using Voat.Domain.Query;
using Voat.Domain.Models;
using System.Threading.Tasks;
using Voat.Domain.Query.Statistics;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryStatisticsTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Statistics")]
        public async Task Query_Stats_HighestVotedContent()
        {
            var q = new QueryHighestVotedContent();
            var r = await q.ExecuteAsync();
            Assert.IsNotNull(r);

        }

        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Statistics")]
        public async Task Query_Stats_UserVotesGiven()
        {
            var q = new QueryUserVotesGiven();
            var r = await q.ExecuteAsync();
            Assert.IsNotNull(r);

        }

        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Statistics")]
        public async Task Query_Stats_UserVotesReceived()
        {
            var q = new QueryUserVotesReceived();
            var r = await q.ExecuteAsync();
            Assert.IsNotNull(r);

        }

       

    }
}
