#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Rules;
using Voat.RulesEngine;
using Voat.Tests.Repository;
using Voat.Utilities;

namespace Voat.Tests.Rules
{
    [TestClass]
    public class RulesTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Rules")]
        public void DownVoat_Comment_Denied_MinCCPInSubverse()
        {
            TestHelper.SetPrincipal("User100CCP");
            var context = new VoatRuleContext();
            context.PropertyBag.CommentID = 5;//A minCCP of 5000 is required in this comment sub
            context.PropertyBag.AddressHash = IpHash.CreateHash("127.0.0.1");
            var outcome = UnitTestRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.DownVoteComment, RuleScope.DownVote, RuleScope.Vote);
            Assert.AreEqual(RuleResult.Denied, outcome.Result);

            Assert.AreEqual("2.4", outcome.RuleNumber);
        }
        
        [TestMethod]
        [TestCategory("Rules")]
        public void DownVoat_Submission_Denied_MinCCPInSubverse()
        {
            TestHelper.SetPrincipal("User100CCP");
            var context = new VoatRuleContext();
            context.PropertyBag.SubmissionID = 3; //A minCCP of 5000 is required in this comment sub

            var outcome = UnitTestRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.DownVoteSubmission, RuleScope.DownVote, RuleScope.Vote);
            Assert.AreEqual(RuleResult.Denied, outcome.Result);
            Assert.AreEqual("2.4", outcome.RuleNumber);
        }

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        [TestCategory("Rules")]
        public void UpVoat_Comment_Allowed()
        {
            //rulesEngine.Context.PropertyBag.UserName = "User50CCP";
            //rulesEngine.Context.PropertyBag.CommentID = 1;
            TestHelper.SetPrincipal("User50CCP");
            var context = new VoatRuleContext();
            context.PropertyBag.CommentID = 1;//A minCCP of 5000 is required in this comment sub

            var outcome = UnitTestRulesEngine.Instance.EvaluateRuleSet(context, RulesEngine.RuleScope.UpVoteComment, true);
            Assert.AreEqual(RuleResult.Allowed, outcome.Result);
        }

        [TestMethod]
        [TestCategory("Rules")]
        public void UpVoat_Submission_Allowed()
        {
            //rulesEngine.Context.PropertyBag.UserName = "User50CCP";
            //rulesEngine.Context.PropertyBag.SubmissionID = 3;
            TestHelper.SetPrincipal("User50CCP");
            var context = new VoatRuleContext();
            context.PropertyBag.SubmissionID = 3;//A minCCP of 5000 is required in this comment sub
            var outcome = UnitTestRulesEngine.Instance.EvaluateRuleSet(context, RulesEngine.RuleScope.UpVoteSubmission, true);
            Assert.AreEqual(RuleResult.Allowed, outcome.Result);
        }
    }
}
