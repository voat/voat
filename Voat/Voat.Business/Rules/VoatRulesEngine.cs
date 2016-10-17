using System;
using Voat.RulesEngine;

namespace Voat.Rules
{
    public class VoatRulesEngine : RulesEngine<VoatRuleContext>
    {
        protected static VoatRulesEngine _engine;

        protected VoatRulesEngine(/*IRequestContextHandler<VoatRequestContext> handler*/) /*: base(handler)*/
        {
        }

        public static VoatRulesEngine Instance
        {
            get
            {
                if (_engine == null)
                {
                    lock (typeof(VoatRulesEngine))
                    {
                        if (_engine == null)
                        {
                            _engine = new VoatRulesEngine(/*new RequestHttpContextHandler()*/);
                            _engine.Initialize(RuleSection.Instance.Configuration);
                            _engine.Enabled = RuleSection.Instance.Configuration.Enabled;
                        }
                    }
                }
                return _engine;
            }

            set
            {
                _engine = value;
            }
        }

        public override RuleOutcome EvaluateRuleSet(VoatRuleContext context, RuleScope[] ruleScopes, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null)
        {
            var outcome = RuleOutcome.Allowed;

            if (this.Enabled)
            {
                outcome = base.EvaluateRuleSet(context, ruleScopes, includeGlobalScope, scopeEvaluator);
            }

            return outcome;
        }

        public RuleOutcome EvaluateRuleSet(VoatRuleContext context, RuleScope scope, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null)
        {
            return EvaluateRuleSet(context, new RuleScope[] { scope }, includeGlobalScope, scopeEvaluator);
        }

        public RuleOutcome EvaluateRuleSet(VoatRuleContext context, params RuleScope[] scopes)
        {
            return EvaluateRuleSet(context, scopes, true, null);
        }
    }
}
