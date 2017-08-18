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
using Voat.Common;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.QueryTests
{
    /// <summary>
    /// Summary description for QueryCommentTests
    /// </summary>
    [TestClass]
    public class QueryCommentTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Comment")]
        public void EnsureVoteSavedIsPopulated()
        {
            var user = TestHelper.SetPrincipal("UnitTestUser18");

            var q = new QueryComment(1).SetUserContext(user);
            var comment = q.Execute();
            Assert.IsNotNull(comment, "Comment is null 1");
            Assert.AreEqual(0, comment.Vote, "vote value not set for logged in user 1");

            var cmd = new CommentVoteCommand(1, 1, IpHash.CreateHash(Guid.NewGuid().ToString())).SetUserContext(user); ;
            var result = cmd.Execute().Result;
            Assert.IsNotNull(result, "Result is null");
            Assert.AreEqual(Status.Success, result.Status);

            q = new QueryComment(1).SetUserContext(user); ;
            comment = q.Execute();
            Assert.IsNotNull(comment, "Comment is null 2");
            Assert.AreEqual(1, comment.Vote, "vote value not set for logged in user 2");
        }

        [TestMethod]
        [TestCategory("Comment"), TestCategory("Comment.Segment")]
        public void Ensure_CommentSegment_Defaults()
        {
            var segment = new CommentSegment();
            Assert.AreEqual(-1, segment.StartingIndex);
            Assert.AreEqual(-1, segment.EndingIndex);
            Assert.AreEqual(0, segment.TotalCount);
            Assert.AreEqual(false, segment.HasMore);
            Assert.AreEqual(0, segment.RemainingCount);

            var nestedComment = new NestedComment();
            Assert.AreNotEqual(null, nestedComment.Children, "Expecting segment to be non-null");
            segment = nestedComment.Children;
            Assert.AreEqual(-1, segment.StartingIndex);
            Assert.AreEqual(-1, segment.EndingIndex);
            Assert.AreEqual(0, segment.TotalCount);
            Assert.AreEqual(false, segment.HasMore);
            Assert.AreEqual(0, segment.RemainingCount);

        }
    }
}
