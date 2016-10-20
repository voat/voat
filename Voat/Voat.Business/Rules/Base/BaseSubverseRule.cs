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
