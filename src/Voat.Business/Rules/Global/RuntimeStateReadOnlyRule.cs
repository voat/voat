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

using Voat.Configuration;
using Voat.RulesEngine;

namespace Voat.Rules.Global
{
    [RuleDiscovery(false, "Approved if runtime setting allows write operations", "approved = (runtime.State != ReadOnly)")]
    public class RuntimeStateReadOnlyRule : VoatRule
    {
        public RuntimeStateReadOnlyRule() : base("ReadOnly Rule", "1.0", RuleScope.Global, 0)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if ((VoatSettings.Instance.RuntimeState & RuntimeStateSetting.Write) < 0)
            {
                return CreateOutcome(RuleResult.Denied, "Runtime is in a ReadOnly state");
            }
            return Allowed;
        }
    }
}
