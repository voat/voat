using Voat.RulesEngine;

namespace Voat.Rules
{
    public abstract class MinimumCCPRule : VoatRule
    {
        public MinimumCCPRule(string name, string number, int minimumCommentPoints, RuleScope scope)
            : base(name, number, scope)
        {
            MinimumCommentPoints = minimumCommentPoints;
        }

        public int MinimumCommentPoints
        {
            get;
            protected set;
        }
    }
}
