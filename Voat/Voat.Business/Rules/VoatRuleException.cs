namespace Voat.Rules
{
    public class VoatRuleException : RulesEngine.RuleException
    {
        public VoatRuleException(string message) : base(message)
        {
            /*no-op*/
        }
    }
}
