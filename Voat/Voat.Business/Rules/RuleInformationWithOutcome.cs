using Voat.RulesEngine;

namespace Voat.Rules
{
    public class RuleInformationWithOutcome
    {
        public RuleInformationWithOutcome(RuleInformation info)
        {
            Info = info;
        }

        public RuleInformation Info
        {
            get;
            private set;
        }

        public RuleOutcome Outcome
        {
            get;
            set;
        }
    }
}
