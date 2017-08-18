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

namespace Voat.Rules.Global
{
    [RuleDiscovery("Approved if user isn't globally banned.", "approved = (user.IsBanned == false)")]
    public class GlobalBanRule : VoatRule
    {
        public GlobalBanRule() : base("Global Ban Rule", "5.1", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var userName = context.UserName;
            if (!string.IsNullOrEmpty(userName))
            {
                if (UserHelper.IsUserGloballyBanned(userName))
                {
                    return CreateOutcome(RuleResult.Denied, "User is globally banned");
                }
            }
            return Allowed;
        }
    }
}
