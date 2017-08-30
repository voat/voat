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

using System;
using Voat.Common;
using Voat.Data;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    public class BaseCommentRule : VoatRule
    {
        public BaseCommentRule(string name, string number, RuleScope scope, int order = 100) : base(name, number, scope, order)
        {
            /*no-op*/
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            string content = context.PropertyBag.CommentContent;
            if (String.IsNullOrWhiteSpace(content))
            {
                return base.CreateOutcome(RuleResult.Denied, "Empty comments not allowed");
            }

            //Was being triggered for edits with only case changes
            if (Scope == RuleScope.PostComment)
            {
                // check for copypasta
                using (var repo = new Repository())
                {
                    var copyPasta = repo.SimilarCommentSubmittedRecently(context.UserName, content.TrimWhiteSpace(), TimeSpan.FromHours(24));
                    if (copyPasta)
                    {
                        return base.CreateOutcome(RuleResult.Denied, "You have recently submitted a similar comment. Please try to not use copy/paste so often.");
                    }
                }
            }

            return Allowed;
        }
    }
}
