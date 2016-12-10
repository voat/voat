using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Domain.Command;
using Voat.Utilities;
using Voat.Domain.Query;
using Voat.Domain.Models;

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
            TestHelper.SetPrincipal("UnitTestUser18");

            var q = new QueryComment(1);
            var comment = q.Execute();
            Assert.IsNotNull(comment, "Comment is null 1");
            Assert.AreEqual(0, comment.Vote, "vote value not set for logged in user 1");

            var cmd = new CommentVoteCommand(1, 1, IpHash.CreateHash(Guid.NewGuid().ToString()));
            var result = cmd.Execute().Result;
            Assert.IsNotNull(result, "Result is null");
            Assert.AreEqual(Status.Success, result.Status);

            q = new QueryComment(1);
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
