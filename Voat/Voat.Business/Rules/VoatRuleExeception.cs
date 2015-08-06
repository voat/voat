namespace Voat.Rules
{
    public class VoatRuleExeception : RulesEngine.RuleException
    {
        public VoatRuleExeception(string message) : base(message)
        {
            /*no-op*/
        }

    }
}
