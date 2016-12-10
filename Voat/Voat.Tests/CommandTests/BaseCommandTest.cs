using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Tests.CommandTests
{
    public class BaseCommandTest : BaseUnitTest
    {

        protected void VerifyCommentSegmentIsAnonProtected(CommentSegment segment)
        {
            if (segment != null && segment.Comments != null)
            {
                foreach (var c in segment.Comments)
                {
                    EnsureAnonIsProtected(c);
                    VerifyCommentSegmentIsAnonProtected(c.Children);
                }
            }
        }
        private void EnsureAnonIsProtected(Domain.Models.Comment comment)
        {
            Assert.AreEqual(true, comment.IsAnonymized, $"Expected anonymized comment on comment {comment.ID}");
            Assert.AreEqual(comment.ID.ToString(), comment.UserName, $"Expected username to be changed on comment {comment.ID}");
        }
        protected void VerifyCommentIsProtected(int submissionID, int commentID, string userName = null)
        {
            if (!String.IsNullOrEmpty(userName))
            {
                TestHelper.SetPrincipal(userName);
            }
            
            //verify comment segment hides user name
            var q = new QueryCommentContext(submissionID, commentID);
            var r = q.Execute();
            Assert.IsNotNull(r, "Query response is null");
            Assert.IsNotNull(r.Comments, "Comment segment is null");
            
            VerifyCommentSegmentIsAnonProtected(r);
            var comment = r.Comments.FirstOrDefault();

            if (!String.IsNullOrEmpty(userName))
            {
                Assert.IsTrue(comment.IsOwner, $"Expected user {userName} to be submitter on comment {comment.ID}");
            }

            //Ensure direct comment is protected
            var q2 = new QueryComment(commentID, CachePolicy.None);
            var r2 = q2.Execute();
            Assert.IsNotNull(r2, "Query 2 response is null");
            EnsureAnonIsProtected(r2);

            //Ensure stream comment is protected
            var options = new SearchOptions();
            options.StartDate = r2.CreationDate.AddMinutes(-5);
            var q3 = new QueryComments(r2.Subverse, options, CachePolicy.None);
            var r3 = q3.Execute();
            Assert.IsNotNull(r3, "Expecting stream endpoint to return comment");
            Assert.AreNotEqual(0, r3.Count(), "Expected at least 1 comment to be returned");
            foreach (var c in r3)
            {
                EnsureAnonIsProtected(c);
            }


        }
    }
}
