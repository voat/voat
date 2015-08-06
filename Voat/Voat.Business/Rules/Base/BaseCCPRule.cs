using System;
using Voat.RulesEngine;

namespace Voat.Rules
{

    /// <summary>
    /// Base class for any simple rules concerning only CCP and a user action.
    /// </summary>
    public abstract class BaseCCPVote : MinCCPRule {

        public BaseCCPVote(string name, string number, int minCCP, RuleScope scope)
            : base(name, number, minCCP, scope) {
        }


        public override RuleOutcome Evaluate() {

            //check if context already has this property computed, if not compute and store it as other rules may query this value
            if (Context.PropertyBag.CCP == null) {
                //using (DataGateway db = new DataGateway()) {
                //    Context.PropertyBag.CCP = db.UserContributionPoints(Context.UserName, ContentType.Comment).Sum;
                //}
            }

            if (Context.PropertyBag.CCP < MinCCP) {
                return CreateOutcome(RuleResult.Denied, (String.Format("CCP of {0} is below minimum of {1} required for action {2}", Context.PropertyBag.CCP, MinCCP, Scope.ToString())));
            }

            return Allowed;
        }

    }
}