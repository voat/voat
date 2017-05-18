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
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment post if data does not contain banned domains.", "approved = (comment.ContainsBannedDomains() == false)")]
    public class PostCommentBannedDomainRule : VoatRule
    {
        public PostCommentBannedDomainRule()
            : base("Comment Banned Domain", "7.4", RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            string content = context.PropertyBag.CommentContent;

            //Check banned domains in submission content
            var containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, content);
            if (containsBannedDomain)
            {
                return CreateOutcome(RuleResult.Denied, "Comment contains banned domains");
            }

            return Allowed;
        }
    }
}
