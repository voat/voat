using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Domain.Command;
using Voat.Utilities;
using Voat.Domain.Query;

namespace Voat.Tests.QueryTests
{
    /// <summary>
    /// Summary description for QueryCommentTests
    /// </summary>
    [TestClass]
    public class QueryCommentTests
    {
        [TestMethod]
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
    }
}
