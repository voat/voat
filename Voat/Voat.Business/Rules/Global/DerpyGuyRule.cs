using Voat.RulesEngine;

namespace Voat.Rules.General
{
    [RuleDiscovery(false, "Approves action if username isn't DerpyGuy", "approved = (user.Name != DerpyGuy)")]
    public class DerpyGuyRule : VoatRule
    {
        public DerpyGuyRule() : base("DerpyGuy", "88.88.89", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (context.UserName == "DerpyGuy")
            {
                return CreateOutcome(RuleResult.Denied, "Your name is DerpyGuy");
            }
            return Allowed;
        }
    }
}
