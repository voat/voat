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
            if ((RuntimeState.Current & RuntimeStateSetting.Write) < 0)
            {
                return CreateOutcome(RuleResult.Denied, "Runtime is in a ReadOnly state");
            }
            return Allowed;
        }
    }
}
