using System;
using System.Collections.Generic;
using Voat.RulesEngine;

namespace Voat.Rules
{

    public class BaseSubverseMinCCPRule : BaseVoatRule {


        public BaseSubverseMinCCPRule(string name, string number, RuleScope scope)
            : base(name, number, scope) {
        }

        public override IDictionary<string, Type> RequiredContext {
            get {
                return new Dictionary<string, Type>() { { "SubverseName", typeof(string) } };
            }
        }
        public override RuleOutcome Evaluate() {

            if (String.IsNullOrEmpty(Context.SubverseName)) {
                
                throw new VoatRuleExeception("SubverseName is a required value for rule evaluation.");

            }
            
            //using (DataGateway db = new DataGateway()) {

            //    var sub = db.GetSubverseInfo(Context.SubverseName);

            //    int? subMinCCP = sub.minimumdownvoteccp;

            //    if (subMinCCP.HasValue && subMinCCP.Value > 0) {

            //        int subverseUserCCP = Karma.CommentKarmaForSubverse(Context.UserName, Context.SubverseName);

            //        if (subverseUserCCP < subMinCCP.Value) {

            //            return CreateOutcome(RuleResult.Denied, String.Format("User {0} has {1} CPP in subverse '{2}' and {3} is required to downvote.", Context.UserName, subverseUserCCP, Context.SubverseName, subMinCCP.Value.ToString()));
                    
            //        }

            //    }
            //}
            return Allowed;
        }
    
    }
}