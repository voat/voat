#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting.Base
{
    //[RuleDiscovery("Approves a submission post if data does not contain banned domains.", "approved = (submission.ContainsBannedDomains() == false)")]
    public class SubmissionBannedDomainRule : VoatRule
    {
        public SubmissionBannedDomainRule(string name, string number, RuleScope scope)
            : base(name, number, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            UserSubmission submission = context.PropertyBag.UserSubmission;

            //Check banned domains in submission content
            var containsBannedDomain = false;
            switch (submission.Type)
            {
                case SubmissionType.Link:
                    containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, $"{submission.Title} {submission.Url}");
                    break;

                case SubmissionType.Text:
                    containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, $"{submission.Title} {submission.Content}");
                    break;
            }
            if (containsBannedDomain)
            {
                return CreateOutcome(RuleResult.Denied, "Submission contains banned domains");
            }

            return Allowed;
        }
    }
}
