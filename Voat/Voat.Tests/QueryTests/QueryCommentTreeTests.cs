using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.Repository;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryCommentTreeTests
    {
        private static int _unitSubmissionID;
        private static int _rootCount = 1;
        private static int _nestedCount = 4;
        private static int _recurseCount = 4;
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _unitSubmissionID = VoatDataInitializer.BuildCommentTree("unit", "Build Comment Tree", _rootCount, _nestedCount, _recurseCount);
        }
        [TestMethod]
        public void GetCommentSegmentWithContext()
        {

            var commentID = 22;
            using (var db = new voatEntities())
            {
                var nestedcomment = db.Comments.Where(x => x.Content.Contains("Path 1:1:1:1:1")).FirstOrDefault();
                if (nestedcomment == null)
                {
                    Assert.Fail("Can not find expected comment in database");
                }
                commentID = nestedcomment.ID;
            }

            TestHelper.SetPrincipal("TestUser1");
            var q = new QueryCommentContext(_unitSubmissionID, commentID, 0, new Voat.Data.SearchOptions() { Sort = SortAlgorithm.New });
            var r = q.Execute();
            Assert.IsNotNull(r, "Query returned null");

            string expectedPath = "Path 1";
            NestedComment comment = r.Comments.First();
            for (int i = 0; i < 5; i++)
            {
                Assert.IsNotNull(comment, "looping comment was null");
                Assert.IsTrue(comment.Content.EndsWith(expectedPath), $"Expected path to end with {expectedPath}, but got {comment.Content}");
                comment = comment.Children != null ? comment.Children.Comments.First() : null;
                expectedPath += ":1";
            }


        }

        [TestMethod]
        public async Task Test_CommentTreeLoading()
        {
            TestHelper.SetPrincipal("TestUser1");
            var q = new QueryCommentSegment(_unitSubmissionID, null, null, new Voat.Data.SearchOptions() { Sort = SortAlgorithm.New });
            var r = q.Execute();

            Assert.IsNotNull(r, "Segment returned null");
            Assert.AreEqual(_rootCount, r.Comments.Count, "expected root count off");
            foreach (var c in r.Comments)
            {
                Assert.IsNull(c.ParentID, "expecting only root comments");
                Assert.AreEqual(_nestedCount, c.ChildCount, "Child Count on Root is off");
                Assert.AreEqual(_unitSubmissionID, c.SubmissionID);

                //Test for embedded objects
                Assert.IsNotNull(c.Children, "Children object is null");
                Assert.IsNotNull(c.Children.Comments, "Children Comments object is null");
                Assert.AreEqual(_nestedCount, c.Children.TotalCount, "Child comment count off in embedded object");

                Action<CommentSegment, int> testSegment = null;
                testSegment = new Action<CommentSegment,int>((segment, expectedCount) => {

                    Assert.IsNotNull(segment);
                    Assert.AreEqual(expectedCount, segment.TotalCount); 
                    foreach (var child in segment.Comments)
                    {
                        Assert.IsNotNull(child, "Comment Null");
                        Assert.AreEqual(_unitSubmissionID, c.SubmissionID, "Submission of Comment doesn't match tree");
                        if (child.Children != null)
                        {
                            testSegment(child.Children, expectedCount);
                        }
                    }

                    //Test individual segment and ensure it matches current segment
                    var q2 = new QueryCommentSegment(_unitSubmissionID, segment.Comments.First().ParentID, null, new Voat.Data.SearchOptions() { Sort = SortAlgorithm.New }); //all children should have same parent
                    var segment2 = q2.Execute();
                    CompareSegments(segment, segment2);


                });
                testSegment(c.Children, _nestedCount);
            }
        }

        public void CompareSegments(CommentSegment expected, CommentSegment actual)
        {
            //compare segments
            Assert.AreEqual(expected.TotalCount, actual.TotalCount, "Segments don't match on child query");
            Assert.AreEqual(expected.SegmentCount, actual.SegmentCount, "Segments don't match on child query");
            Assert.AreEqual(expected.TotalCount, actual.TotalCount, "Segments don't match on child query");
            Assert.AreEqual(expected.Comments.Count, actual.Comments.Count, "Segments don't match on child query");

            for (int i = 0; i < expected.TotalCount; i++)
            {
                var eComment = expected.Comments[i];
                var aComment = actual.Comments[i];
                Assert.AreEqual(eComment.ID, aComment.ID);
                Assert.AreEqual(eComment.UserName, aComment.UserName);
                Assert.AreEqual(eComment.Content, aComment.Content);
            }
        }
    }
}
