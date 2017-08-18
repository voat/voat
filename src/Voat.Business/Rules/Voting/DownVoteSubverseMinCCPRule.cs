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

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approves submission downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.", "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class DownVoteSubverseMinCCPRule : BaseSubverseMinimumCCPRule
    {
        public DownVoteSubverseMinCCPRule()
            : base("Subverse Min CCP Downvote", "2.4", RuleScope.DownVote)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }
}
