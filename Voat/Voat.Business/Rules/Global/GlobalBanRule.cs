using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Global
{
    [RuleDiscovery("Approved if user isn't globally banned.", "approved = (user.IsBanned == false)")]
    public class DerpyGuyRule : VoatRule
    {
        public DerpyGuyRule() : base("Global Ban Rule", "5.1", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (UserHelper.IsUserGloballyBanned(context.UserName))
            {
                return CreateOutcome(RuleResult.Denied, "User is globally banned");
            }
            return Allowed;
        }
    }
}
