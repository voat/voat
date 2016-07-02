using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Tests.Data;

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

            //These users need to match the datainitializer batch setup
            int start = 0;
            int end = 50;

            var userName = String.Format(UNIT_TEST_CONSTANTS.UNIT_TEST_USER_TEMPLATE, "0");

            if (context != null)
            {
                userName = context.UserName;
            }

            //increment user name
            var match = Regex.Match(userName, @"\d+").Value;
            int numeric = start;
            if (int.TryParse(match, out numeric))
            {
                numeric += 1;
            }
            if (numeric > end)
            {
                numeric = start;
            }
            userName = String.Format(UNIT_TEST_CONSTANTS.UNIT_TEST_USER_TEMPLATE, numeric.ToString());

            TestHelper.SetPrincipal(userName);
            if (createData)
            {
                using (var db = new Voat.Data.Repository())
                {
                    var m = db.PostSubmission(new UserSubmission() { Subverse="unit", Title = "Test Post for Unit Testing", Content = "Test Content" }).Result;
                    Assert.AreEqual(Status.Success, m.Status, String.Format("NewContext PostSubmission for user {0} received non-success message : {1}", userName, m.Message));
                    var submissionid = m.Response.ID;

                    var c = db.PostComment(submissionid, -1, "This is a comment + " + Guid.NewGuid().ToString()).Result;
                    Assert.AreEqual(Status.Success, c.Status, String.Format("NewContext PostComment for user {0} received non-success message : {1}", userName, m.Message));
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
