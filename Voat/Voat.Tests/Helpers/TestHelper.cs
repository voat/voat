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
using System.Security.Principal;
using Voat.Common;
using Voat.Domain.Command;

namespace Voat.Tests
{
    public class TestHelper
    {
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


        public static class ContentCreation {

            public static Domain.Models.Submission CreateSubmission(string userName, Domain.Models.UserSubmission submission)
            {
                var user = TestHelper.SetPrincipal(userName);

                var cmd = new CreateSubmissionCommand(submission).SetUserContext(user);

                var r = cmd.Execute().Result;

                VoatAssert.IsValid(r);
                Assert.AreNotEqual(0, r.Response.ID);

                return r.Response;
            }

            public static Domain.Models.Comment CreateComment(string userName, int submissionID, string content,  int? parentCommentID = null)
            {
                var user = TestHelper.SetPrincipal(userName);
                var cmd = new CreateCommentCommand(submissionID, parentCommentID, content).SetUserContext(user);
                var c = cmd.Execute().Result;
                Assert.IsTrue(c.Success);
                Assert.IsNotNull(c.Response);
                Assert.AreNotEqual(0, c.Response.ID);
                return c.Response;
            }
        }
    }
}
