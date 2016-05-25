using Voat.Data.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    public abstract class BaseSubverseBanRule : VoatRule
    {
        public BaseSubverseBanRule(string name, string number, RuleScope scope)
            : base(name, number, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            DemandContext(context);

            Subverse subverse = context.PropertyBag.Subverse;

            if (UserHelper.IsUserBannedFromSubverse(context.UserName, subverse.Name))
            {
                return CreateOutcome(RuleResult.Denied, "User is banned from v/{0}", subverse.Name);
            }

            return base.EvaluateRule(context);
        }
    }
}
