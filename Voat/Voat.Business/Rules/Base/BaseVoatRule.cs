using Voat.RulesEngine;

namespace Voat.Rules
{
    public abstract class BaseVoatRule : Rule {

        public BaseVoatRule(string name, string number, RuleScope scope, int order = 100) : base(name, number, scope, order) { 
            /*no-op*/
        }

        public VoatRuleContext Context {
            get {
                return VoatRulesEngine.Instance.Context;
            }
        }
    
    }
}