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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.Infrastructure;

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
            else
            {
                Assert.Fail("segment is null or has no comments");
            }
        }
        private void EnsureAnonIsProtected(Domain.Models.Comment comment)
        {
            Assert.AreEqual(true, comment.IsAnonymized, $"Expected anonymized comment on comment {comment.ID}");
            Assert.AreEqual(comment.ID.ToString(), comment.UserName, $"Expected username to be changed on comment {comment.ID}");
        }
        protected void VerifyCommentIsProtected(int submissionID, int commentID, string userName = null)
        {
            IPrincipal user = null;
            if (!String.IsNullOrEmpty(userName))
            {
                user = TestHelper.SetPrincipal(userName);
            }
            
            //verify comment segment hides user name
            var q = new QueryCommentContext(submissionID, commentID).SetUserContext(user);
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
        public T EnsureCommandIsSerialziable<T>(T command) where T : Command
        {
            var json = JsonConvert.SerializeObject(command, JsonSettings.DataSerializationSettings);
            var deserializedCmd = (T)JsonConvert.DeserializeObject(json, JsonSettings.DataSerializationSettings);

            Assert.AreEqual(command.Context.User.Identity.Name, deserializedCmd.Context.User.Identity.Name);
            Assert.AreEqual(command.User.Identity.Name, deserializedCmd.User.Identity.Name);

            var json2 = JsonConvert.SerializeObject(deserializedCmd, JsonSettings.DataSerializationSettings);
            Assert.AreEqual(json, json2);

            return deserializedCmd;
        }
    }
}
