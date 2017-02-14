using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Principal;
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
        public static void SetPrincipal(string name, string[] roles = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                System.Threading.Thread.CurrentPrincipal = null;
            }
            else
            {
                GenericPrincipal p = new GenericPrincipal(new GenericIdentity(name), roles);
                System.Threading.Thread.CurrentPrincipal = p;
            }
        }


        public static class ContentCreation {

            public static Domain.Models.Submission CreateSubmission(string userName, Domain.Models.UserSubmission submission)
            {
                TestHelper.SetPrincipal(userName);

                var cmd = new CreateSubmissionCommand(submission);

                var r = cmd.Execute().Result;

                Assert.IsNotNull(r, "Response is null");
                Assert.IsTrue(r.Success, r.Message);
                Assert.IsNotNull(r.Response, "Expecting a non null response");
                Assert.AreNotEqual(0, r.Response.ID);

                return r.Response;
            }

            public static Domain.Models.Comment CreateComment(string userName, int submissionID, string content,  int? parentCommentID = null)
            {
                TestHelper.SetPrincipal(userName);
                var cmd = new CreateCommentCommand(submissionID, parentCommentID, content);
                var c = cmd.Execute().Result;
                Assert.IsTrue(c.Success);
                Assert.IsNotNull(c.Response);
                Assert.AreNotEqual(0, c.Response.ID);
                return c.Response;
            }
        }
    }
}
