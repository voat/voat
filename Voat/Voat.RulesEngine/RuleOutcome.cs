using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.RulesEngine
{

    /// <summary>
    /// The resulting output of any rule evaluation. If result isn't Allowed, error information will be included about he denial.
    /// </summary>
    public class RuleOutcome {

        public RuleOutcome(RuleResult result, string ruleName, string ruleNumber, string message) {
            this.Result = result;
            this.Message = message;
            this.RuleName = ruleName;
            this.RuleNumber = ruleNumber;
        }

        public RuleResult Result {
            get;
            set;
        }
        public string Message {
            get;
            set;
        }
        public string RuleName {
            get;
            set;
        }
        public string RuleNumber {
            get;
            set;
        }
        public bool IsAllowed {
            get {
                return Result == RuleResult.Allowed || Result == RuleResult.Unevaluated;
            }
        }
        public bool IsDenied {
            get {
                return Result == RuleResult.Denied;
            }
        }
        public override string ToString() {
            return String.Format("{2}. {0} ({1}): {3}", RuleName, RuleNumber, Result.ToString(), Message);
        }

        public static RuleOutcome Allowed {
            get {
                return new RuleOutcome(RuleResult.Allowed, "Global", "0.0", "Action Allowed");
            }
        }
    }
}