using Voat.RulesEngine;

namespace Voat.Rules
{
    public abstract class VoatRule : Rule<VoatRuleContext>
    {
        public VoatRule(string name, string number, RuleScope scope, int order = 100) : base(name, number, scope, order)
        {
            /*no-op*/
        }

        /// <summary>
        /// Mostly for debugging to ensure rule context has necessary data to process requests.
        /// </summary>
        /// <param name="value"></param>
        protected void DemandContext(object value)
        {
            if (value == null)
            {
                throw new VoatRuleException("Specified required value is not set");
            }
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return Allowed;
        }
    }
}
