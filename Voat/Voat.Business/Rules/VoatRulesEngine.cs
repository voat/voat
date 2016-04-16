using System;
using System.Diagnostics;
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
            Debug.Print("~~~~~ RULE SET EVAL ~~~~~~~");
            Debug.Print("Scope: {0}", ruleScopes.ToString());
            Debug.Print("PRE EVAL CONTEXT ----------");
            Debug.Print(context.ToString());
            Debug.Print("-");
            var outcome = base.EvaluateRuleSet(context, ruleScopes, includeGlobalScope, scopeEvaluator);
            Debug.Print("POST EVAL CONTEXT  --------");
            Debug.Print(context.ToString());
            Debug.Print("Outcome: {0}", outcome.Result.ToString());
            Debug.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Debug.Print("");

            return outcome;
        }

        public RuleOutcome EvaluateRuleSet(VoatRuleContext context, RuleScope scope, bool includeGlobalScope = true, Func<Rule, RuleScope, bool> scopeEvaluator = null)
        {
            return EvaluateRuleSet(context, new RuleScope[] { scope }, includeGlobalScope, scopeEvaluator);
        }
        
    }
}
