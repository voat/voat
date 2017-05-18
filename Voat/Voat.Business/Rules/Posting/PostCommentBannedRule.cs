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

using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class PostCommentBannedRule : BaseSubverseRule
    {
        public PostCommentBannedRule()
            : base("Subverse Comment Ban Rule", "5.3", RuleScope.PostComment)
        {
            this.Order = 1;
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //base class will check for subverse and global bans here
            return base.EvaluateRule(context);
        }
    }
}
