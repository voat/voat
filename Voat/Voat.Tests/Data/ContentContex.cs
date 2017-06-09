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

            var userName = String.Format(UNIT_TEST_CONSTANTS.UNIT_TEST_USER_TEMPLATE, "0".PadLeft(2, '0'));

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
            userName = String.Format(UNIT_TEST_CONSTANTS.UNIT_TEST_USER_TEMPLATE, numeric.ToString().PadLeft(2, '0'));

            var user = TestHelper.SetPrincipal(userName);
            if (createData)
            {
                using (var db = new Voat.Data.Repository(user))
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
