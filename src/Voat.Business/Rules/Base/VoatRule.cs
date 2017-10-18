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
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.RulesEngine;

namespace Voat.Rules
{
    public abstract class VoatRule : Rule<VoatRuleContext>
    {
        public VoatRule(string name, string number, RuleScope scope, int order = 100) : base(name, number, scope, order)
        {
            /*no-op*/
        }

        /// <summary>
        /// Mostly for debugging to ensure rule context has necessary data to process requests.
        /// </summary>
        /// <param name="value"></param>
        protected void DemandContext(object value)
        {
            if (value == null)
            {
                throw new VoatRuleException("Specified required value is not set");
            }
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return Allowed;
        }

        #region ThrottleLogic


        // check if a given user has used his daily posting quota for a given subverse
        protected bool UserDailyPostingQuotaForSubUsed(VoatRuleContext context, string subverse)
        {
            // read daily posting quota per sub configuration parameter from web.config
            int limit = VoatSettings.Instance.DailyPostingQuotaPerSub;

            using (var repo = new Repository())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Submission, subverse, DateRange.StartFrom(TimeSpan.FromHours(24)));
                //var userSubmissionsToTargetSub = repo.UserSubmissionCount(context.UserName, TimeSpan.FromHours(24), null, subverse);

                if (limit <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily posting quota
        protected bool UserDailyPostingQuotaForNegativeScoreUsed(VoatRuleContext context)
        {
            int limit = VoatSettings.Instance.DailyPostingQuotaForNegativeScore;

            using (var repo = new Repository())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Submission,null, DateRange.StartFrom(TimeSpan.FromHours(24)));
                
                if (limit <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily comment posting quota
        protected bool UserDailyCommentPostingQuotaForNegativeScoreUsed(VoatRuleContext context)
        {
            // read daily posting quota per sub configuration parameter from web.config
            int limit = VoatSettings.Instance.DailyCommentPostingQuotaForNegativeScore;

            using (var repo = new Repository())
            {
                // check how many submission user made today
                var userCommentCountInPast24Hours = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Comment, null, DateRange.StartFrom(TimeSpan.FromHours(24))); 
                    
                if (limit <= userCommentCountInPast24Hours)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily comment posting quota
        protected bool UserDailyCommentPostingQuotaUsed(VoatRuleContext context)
        {
            // read daily posting quota per sub configuration parameter from web.config
            int limit = VoatSettings.Instance.DailyCommentPostingQuota;

            using (var repo = new Repository())
            {
                // check how many submission user made today
                var userCommentSubmissionsInPast24Hours = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Comment, null, DateRange.StartFrom(TimeSpan.FromHours(24))); 
                    
                if (limit <= userCommentSubmissionsInPast24Hours)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his hourly comment posting quota
        protected bool UserHourlyCommentPostingQuotaUsed(VoatRuleContext context)
        {
            // read hourly posting quota configuration parameter from web.config
            int limit = VoatSettings.Instance.HourlyCommentPostingQuota;

            using (var repo = new Repository())
            {
                // check how many comments user made in the last 59 minutes
                var userCommentSubmissionsInPastHour = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Comment, null, DateRange.StartFrom(TimeSpan.FromHours(1)));
                    
                if (limit <= userCommentSubmissionsInPastHour)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his hourly posting quota for a given subverse
        protected bool UserHourlyPostingQuotaForSubUsed(VoatRuleContext context, string subverse)
        {
            // read daily posting quota per sub configuration parameter from web.config
            int limit = VoatSettings.Instance.HourlyPostingQuotaPerSub;

            using (var repo = new Repository())
            {
                // check how many submission user made in the last hour
                var userSubmissionsToTargetSub = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Submission, subverse, DateRange.StartFrom(TimeSpan.FromHours(1)));

                if (limit <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his global hourly posting quota
        protected bool UserHourlyGlobalPostingQuotaUsed(VoatRuleContext context)
        {
            //DRY: Repeat Block #1
            // only execute this check if user account is less than a month old and user SCP is less than 50 and user is not posting to a sub they own/moderate
            DateTime userRegistrationDateTime = context.UserData.Information.RegistrationDate;
            int memberInDays = (Repository.CurrentDate - userRegistrationDateTime).Days;
            if (memberInDays > 30)
            {
                return false;
            }
            else
            {
                int userScp = context.UserData.Information.SubmissionPoints.Sum;
                if (userScp >= 50)
                {
                    return false;
                }
            }

            // read daily posting quota per sub configuration parameter from web.config
            int limit = VoatSettings.Instance.HourlyGlobalPostingQuota;

            using (var repo = new Repository())
            {
                // check how many submission user made in the last hour
                var totalUserSubmissionsForTimeSpam = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Submission, null, DateRange.StartFrom(TimeSpan.FromHours(1))); 

                if (limit <= totalUserSubmissionsForTimeSpam)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his global daily posting quota
        protected bool UserDailyGlobalPostingQuotaUsed(VoatRuleContext context)
        {
            //DRY: Repeat Block #1
            // only execute this check if user account is less than a month old and user SCP is less than 50 and user is not posting to a sub they own/moderate
            DateTime userRegistrationDateTime = context.UserData.Information.RegistrationDate;
            int memberInDays = (Repository.CurrentDate - userRegistrationDateTime).Days;
            if (memberInDays > 30)
            {
                return false;
            }
            else
            {
                int userScp = context.UserData.Information.SubmissionPoints.Sum;
                if (userScp >= 50)
                {
                    return false;
                }
            }

            // read daily global posting quota configuration parameter from web.config
            int limit = VoatSettings.Instance.DailyGlobalPostingQuota;

            using (var repo = new Repository())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = repo.UserContributionCount(context.UserName, Domain.Models.ContentType.Submission, null, DateRange.StartFrom(TimeSpan.FromHours(24)));

                if (limit <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if given user has submitted the same url before
        protected bool DailyCrossPostingQuotaUsed(VoatRuleContext context, string url)
        {
            // read daily crosspost quota from web.config
            int dailyCrossPostQuota = VoatSettings.Instance.DailyCrossPostingQuota;

            using (var repo = new Repository())
            {
                var numberOfTimesSubmitted = repo.FindUserLinkSubmissionCount(context.UserName, url, TimeSpan.FromHours(24));

                if (dailyCrossPostQuota <= numberOfTimesSubmitted)
                {
                    return true;
                }
                return false;
            }
        }

        #endregion
    }
}
