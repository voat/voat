using Voat.RulesEngine;

namespace Voat.Rules
{

    public abstract class MinCCPRule : BaseVoatRule {

        public MinCCPRule(string name, string number, int minCCP, RuleScope scope)
            : base(name, number, scope) {
            MinCCP = minCCP;
        }

        public int MinCCP {
            get;
            protected set;
        }

    }
}