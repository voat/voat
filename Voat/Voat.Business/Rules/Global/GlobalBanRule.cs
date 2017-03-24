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
