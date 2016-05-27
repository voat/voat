#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using System;

namespace Voat.RulesEngine
{
    /// <summary>
    /// The resulting output of any rule evaluation. If result isn't Allowed, error information will be included about he denial.
    /// </summary>
    public class RuleOutcome
    {
        public RuleOutcome(RuleResult result, string ruleName, string ruleNumber, string message)
        {
            this.Result = result;
            this.Message = message;
            this.RuleName = ruleName;
            this.RuleNumber = ruleNumber;
        }

        public RuleResult Result
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string RuleName
        {
            get;
            set;
        }

        public string RuleNumber
        {
            get;
            set;
        }

        public bool IsAllowed
        {
            get
            {
                return Result == RuleResult.Allowed || Result == RuleResult.Unevaluated;
            }
        }

        public bool IsDenied
        {
            get
            {
                return Result == RuleResult.Denied;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} ({2} {3})", Result.ToString(), Message, RuleName, RuleNumber);
        }

        public static RuleOutcome Allowed
        {
            get
            {
                return new RuleOutcome(RuleResult.Allowed, "Global", "0.0", "Action Allowed");
            }
        }
    }
}
