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

using Voat.Data.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    public abstract class BaseSubverseRule : VoatRule
    {
        public BaseSubverseRule(string name, string number, RuleScope scope)
            : base(name, number, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            DemandContext(context);

            Subverse subverse = context.PropertyBag.Subverse;
            if (((int)base.Scope & (int)RuleAction.Create) > 0)
            {
                if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                {
                    return CreateOutcome(RuleResult.Denied, "Subverse is disabled");
                }
            }
            if (UserHelper.IsUserBannedFromSubverse(context.UserName, subverse.Name))
            {
                return CreateOutcome(RuleResult.Denied, "User is banned from v/{0}", subverse.Name);
            }

            return base.EvaluateRule(context);
        }
    }
}
