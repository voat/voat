using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Tests.CommandTests
{
    public class BaseCommandTest
    {

        protected void VerifyCommentSegmentIsAnonProtected(CommentSegment segment)
        {
            if (segment != null && segment.Comments != null)
            {
                foreach (var c in segment.Comments)
                {
                    Assert.AreEqual(true, c.IsAnonymized, $"Expected anonymized comment on comment {c.ID}");
                    Assert.AreEqual(c.ID.ToString(), c.UserName, $"Expected username to be changed on comment {c.ID}");
                    VerifyCommentSegmentIsAnonProtected(c.Children);
                }
            }
        }

        protected void VerifyCommentContextIsProtected(int submissionID, int commentID, string userName = null)
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
        }
    }
}
