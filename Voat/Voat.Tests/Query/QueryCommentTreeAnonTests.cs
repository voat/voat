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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.CommandTests;
using Voat.Tests.Infrastructure;
using Voat.Tests.Repository;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryCommentTreeAnonTests : BaseCommandTest
    {
        private static int _submissionID;
        private int _rootCount = 1;
        private int _nestedCount = 4;
        private int _recurseCount = 4;
        public override void ClassInitialize()
        {
            _submissionID = TestDataInitializer.BuildCommentTree("anon", "Build Comment Tree", 1, 2, 2);
        }

        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Anon"), TestCategory("Comment"), TestCategory("Comment.Segment")]
        public void CommentSegment_Anon()
        {
            var user = TestHelper.SetPrincipal("TestUser01");
            var q = new QueryCommentSegment(_submissionID, null, 0, CommentSortAlgorithm.New);
            var r = q.Execute();
            Assert.IsNotNull(r, "Query returned null");

            VerifyCommentSegmentIsAnonProtected(r);
        }


        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Anon"), TestCategory("Comment"), TestCategory("Comment.Segment")]
        public void CommentContext_Anon()
        {
            int commentID = 0;
            using (var db = new VoatDataContext())
            {
                var nestedcomment = db.Comment.Where(x => x.SubmissionID == _submissionID).OrderByDescending(x => x.ID).FirstOrDefault();
                if (nestedcomment == null)
                {
                    Assert.Fail("Can not find expected comment in database");
                }
                commentID = nestedcomment.ID;
            }

            var user = TestHelper.SetPrincipal("TestUser01");
            var q = new QueryCommentContext(_submissionID, commentID, 0, CommentSortAlgorithm.New);
            var r = q.Execute();
            Assert.IsNotNull(r, "Query returned null");

            VerifyCommentSegmentIsAnonProtected(r);
        }
    }
}
