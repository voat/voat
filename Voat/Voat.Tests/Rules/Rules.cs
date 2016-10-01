using Voat.Common;
using Voat.RulesEngine;

namespace Voat.Tests.Rules
{
    [RuleDiscovery(false)]
    public class TestDeniedRule : TestRule
    {
        public TestDeniedRule() : base("Test Denial", "1.1", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(RequestContext context)
        {
            return CreateOutcome(RuleResult.Denied, "Test denied");
        }
    }

    [RuleDiscovery(true)]
    public class TestPassRule : TestRule
    {
        public TestPassRule() : base("Test Pass", "1.0", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(RequestContext context)
        {
            return Allowed;
        }
    }

    public class TestRule : Rule<RequestContext>
    {
        public TestRule(string name, string number, RuleScope scope) : base(name, number, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(RequestContext context)
        {
            //context.PropertyBag
            return Allowed;
        }
    }
}
