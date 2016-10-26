using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.CommandTests;
using Voat.Tests.Repository;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryCommentTreeAnonTests : BaseCommandTest
    {
        private static int _submissionID;
        private static int _rootCount = 1;
        private static int _nestedCount = 4;
        private static int _recurseCount = 4;
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _submissionID = VoatDataInitializer.BuildCommentTree("anon", "Build Comment Tree", 1, 2, 2);
        }

        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Anon"), TestCategory("Comment"), TestCategory("Comment.Segment")]
        public void CommentSegment_Anon()
        {
            TestHelper.SetPrincipal("TestUser1");
            var q = new QueryCommentSegment(_submissionID, null, 0, CommentSortAlgorithm.New);
            var r = q.Execute();
            Assert.IsNotNull(r, "Query returned null");

            VerifyCommentSegmentIsAnonProtected(r);
        }


        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Anon"), TestCategory("Comment"), TestCategory("Comment.Segment")]
        public void CommentContext_Anon()
        {
            int commentID = 0;
            using (var db = new voatEntities())
            {
                var nestedcomment = db.Comments.Where(x => x.SubmissionID == _submissionID).OrderByDescending(x => x.ID).FirstOrDefault();
                if (nestedcomment == null)
                {
                    Assert.Fail("Can not find expected comment in database");
                }
                commentID = nestedcomment.ID;
            }

            TestHelper.SetPrincipal("TestUser1");
            var q = new QueryCommentContext(_submissionID, commentID, 0, CommentSortAlgorithm.New);
            var r = q.Execute();
            Assert.IsNotNull(r, "Query returned null");

            VerifyCommentSegmentIsAnonProtected(r);
        }
    }
}
