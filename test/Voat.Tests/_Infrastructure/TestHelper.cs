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
using System.Security.Principal;
using System.Text.RegularExpressions;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Tests.Infrastructure
{
    public class TestHelper
    {
        private static int _nextUserLastID = 0;
        private static object _lock = new object();

        public static IPrincipal User
        {
            get
            {
                return System.Threading.Thread.CurrentPrincipal;
            }
        }

        /// <summary>
        /// Sets the current threads User Context for unit tests.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roles"></param>
        public static IPrincipal SetPrincipal(string name, params string[] roles)
        {
            IPrincipal principal = null;
            if (string.IsNullOrEmpty(name))
            {
                principal = new GenericPrincipal(new GenericIdentity(""), null); ;
            }
            else
            {
                principal = new GenericPrincipal(new GenericIdentity(name), roles);
                System.Threading.Thread.CurrentPrincipal = principal;
            }
            return principal;
        }

        public static string NextUserName()
        {
            lock (_lock)
            {
                //These users need to match the datainitializer batch setup
                int minID = 0;
                int maxID = 50;

                var nextID = _nextUserLastID + 1;
                if (nextID > maxID)
                {
                    nextID = minID;
                }
                _nextUserLastID = nextID;

                var nextUserName = String.Format(CONSTANTS.UNIT_TEST_USER_TEMPLATE, nextID.ToString().PadLeft(2, '0'));

                return nextUserName;
            }
        }

        public static class ContentCreation
        {
            public static Domain.Models.Submission CreateSubmission(string userName, Domain.Models.UserSubmission submission)
            {
                var user = TestHelper.SetPrincipal(userName);

                var cmd = new CreateSubmissionCommand(submission).SetUserContext(user);
                var r = cmd.Execute().Result;

                VoatAssert.IsValid(r);
                Assert.AreNotEqual(0, r.Response.ID);

                return r.Response;
            }

            public static Domain.Models.Comment CreateComment(string userName, int submissionID, string content, int? parentCommentID = null)
            {
                var user = TestHelper.SetPrincipal(userName);
                var cmd = new CreateCommentCommand(submissionID, parentCommentID, content).SetUserContext(user);
                var c = cmd.Execute().Result;

                VoatAssert.IsValid(c);
                Assert.AreNotEqual(0, c.Response.ID);

                return c.Response;
            }
        }
        public class ContentContext
        {
            private static ContentContext Instance { get; set; }

            public int CommentID { get; set; }
            public int SubmissionID { get; set; }
            public string UserName { get; set; }

            public static ContentContext Create(bool createData = false)
            {
                var context = Instance;

                var userName = NextUserName();

                var user = TestHelper.SetPrincipal(userName);
                if (createData)
                {
                    using (var db = new Voat.Data.Repository(user))
                    {
                        var m = db.PostSubmission(new UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Test Post for Unit Testing", Content = "Test Content" }).Result;
                        Assert.AreEqual(Status.Success, m.Status, String.Format("NewContext PostSubmission for user {0} received non-success message : {1}", userName, m.Message));
                        var submissionid = m.Response.ID;

                        var c = db.PostComment(submissionid, null, "This is a comment in disappearing ink. Wait for it.... -> " + Guid.NewGuid().ToString()).Result;
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
}
