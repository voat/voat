using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Tests.Repository
{
    public class ContentContext
    {
        public static ContentContext Instance { get; set; }
        public int CommentID { get; set; }
        public int SubmissionID { get; set; }
        public string UserName { get; set; }

        public static ContentContext NewContext(bool createData = false)
        {
            var context = Instance;
            List<string> userNames = new List<string> {
                "TestUser4",
                "TestUser5",
                "TestUser6",
                "TestUser7",
                "TestUser8",
                "TestUser9",
                "TestUser10",
                "TestUser11",
                "TestUser12",
                "TestUser13",
                "TestUser14",
                "TestUser15"
            };
            var userName = "TestUser1";

            if (context != null)
            {
                var index = userNames.IndexOf(ContentContext.Instance.UserName);
                index += 1;
                if (index > userNames.Count - 1)
                {
                    index = 0;
                }
                userName = userNames[index];
            }

            TestHelper.SetPrincipal(userName);
            if (createData)
            {
                using (var db = new Voat.Data.Repository())
                {
                    var m = db.PostSubmission("unit", new UserSubmission() { Title = "Test Post", Content = "Test Content" });
                    Assert.AreEqual(Status.Success, m.Status, String.Format("NewContext PostSubmission for user {0} received non-success message", userName));
                    var submissionid = m.Response.ID;

                    var c = db.PostComment(submissionid, -1, "This is a comment");
                    Assert.AreEqual(Status.Success, c.Status, String.Format("NewContext PostComment for user {0} received non-success message", userName));
                    var commentid = c.Response.ID;
                    context = new ContentContext() { UserName = userName, CommentID = commentid, SubmissionID = submissionid };
                }
            }
            else
            {
                context = new ContentContext() { UserName = userName, CommentID = -1, SubmissionID = -1 };
            }
            Instance = context;
            return context;
        }
    }
}
