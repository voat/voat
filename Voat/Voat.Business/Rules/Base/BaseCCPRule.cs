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
using Voat.RulesEngine;

namespace Voat.Rules
{
    /// <summary>
    /// Base class for any simple rules concerning only CCP and a user action.
    /// </summary>
    public abstract class BaseCCPVote : MinimumCCPRule
    {
        public BaseCCPVote(string name, string number, int minCCP, RuleScope scope)
            : base(name, number, minCCP, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (context.UserData.Information.CommentPoints.Sum < MinimumCommentPoints)
            {
                return CreateOutcome(RuleResult.Denied, (String.Format("CCP of {0} is below minimum of {1} required for action {2}", context.UserData.Information.CommentPoints.Sum, MinimumCommentPoints, Scope.ToString())));
            }
            return base.EvaluateRule(context);
        }
    }
}
