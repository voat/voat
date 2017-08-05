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
using System.Linq;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    //TODO: This rule need refactored, too many db calls.
    [RuleDiscovery("Approved if a user doesn't surpass Voat's submission throttling policy.", "approved = !(throttlePolicy.Surpassed = true)")]
    public class PostSubmissionThrottleRule : VoatRule
    {
        public PostSubmissionThrottleRule()
            : base("Submission Throttle", "4.1", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //bool isModerator = context.UserData.Information.Moderates.Any(x => x.Subverse.Equals(context.Subverse.Name, StringComparison.OrdinalIgnoreCase));
            bool isModerator = ModeratorPermission.IsModerator(context.User, context.Subverse.Name, null);

            // check posting quotas if user is posting to subs they do not moderate
            if (!isModerator)
            {
                // reject if user has reached global daily submission quota
                if (UserDailyGlobalPostingQuotaUsed(context))
                {
                    return CreateOutcome(RuleResult.Denied, "You have reached your daily global submission quota");
                }

                // reject if user has reached global hourly submission quota
                if (UserHourlyGlobalPostingQuotaUsed(context))
                {
                    return CreateOutcome(RuleResult.Denied, "You have reached your hourly global submission quota");
                }

                // check if user has reached hourly posting quota for target subverse
                if (UserHourlyPostingQuotaForSubUsed(context, context.Subverse.Name))
                {
                    return CreateOutcome(RuleResult.Denied, "You have reached your hourly submission quota for this subverse");
                }

                // check if user has reached daily posting quota for target subverse
                if (UserDailyPostingQuotaForSubUsed(context, context.Subverse.Name))
                {
                    return CreateOutcome(RuleResult.Denied, "You have reached your daily submission quota for this subverse");
                }
                if (context.Subverse.IsAuthorizedOnly)
                {
                    return CreateOutcome(RuleResult.Denied, "You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization");
                }
            }

            Domain.Models.UserSubmission userSubmission = context.PropertyBag.UserSubmission;
            if (userSubmission.Type == Domain.Models.SubmissionType.Link)
            {
                using (var repo = new Data.Repository())
                {
                    int crossPostCount = repo.FindUserLinkSubmissionCount(context.UserName, userSubmission.Url, TimeSpan.FromDays(1));
                    if (crossPostCount >= VoatSettings.Instance.DailyCrossPostingQuota)
                    {
                        return CreateOutcome(RuleResult.Denied, "You have reached your daily crossposting quota for this Url");
                    }
                }

                //Old code
                //if (UserHelper.DailyCrossPostingQuotaUsed(userName, submissionModel.Content))
                //{
                //    // ABORT
                //    return ("You have reached your daily crossposting quota for this URL.");
                //}
            }

            return Allowed;
        }
    }
}
