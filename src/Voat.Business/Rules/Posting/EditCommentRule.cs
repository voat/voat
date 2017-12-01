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
    [RuleDiscovery("Approves a comment edit if data is acceptable.", "approved = (comment.IsValid())")]
    public class EditCommentRule : BaseCommentRule
    {
        public EditCommentRule()
            : base("Edit Comment", "7.6", RuleScope.EditComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            Data.Models.Comment comment = context.PropertyBag.Comment;

            if (comment.IsDeleted)
            {
                return base.CreateOutcome(RuleResult.Denied, "Deleted comments can not be edited");
            }
            if (comment.UserName != context.UserName)
            {
                return base.CreateOutcome(RuleResult.Denied, "User does not have permissions to perform requested action");
            }

            //rules checkd in base class
            return base.EvaluateRule(context);
        }
    }
}
