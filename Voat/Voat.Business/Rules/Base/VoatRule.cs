using System;
using System.Linq;
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
            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.DailyPostingQuotaPerSub;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = db.Submissions.Count(
                    m => m.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        && m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily posting quota
        protected bool UserDailyPostingQuotaForNegativeScoreUsed(VoatRuleContext context)
        {
            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.DailyPostingQuotaForNegativeScore;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userSubmissionsInPast24Hours = db.Submissions.Count(
                    m => m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= userSubmissionsInPast24Hours)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily comment posting quota
        protected bool UserDailyCommentPostingQuotaForNegativeScoreUsed(VoatRuleContext context)
        {
            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.DailyCommentPostingQuotaForNegativeScore;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userCommentSubmissionsInPast24Hours = db.Comments.Count(
                    m => m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= userCommentSubmissionsInPast24Hours)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily comment posting quota
        protected bool UserDailyCommentPostingQuotaUsed(VoatRuleContext context)
        {
            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.DailyCommentPostingQuota;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userCommentSubmissionsInPast24Hours = db.Comments.Count(
                    m => m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= userCommentSubmissionsInPast24Hours)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his hourly comment posting quota
        protected bool UserHourlyCommentPostingQuotaUsed(VoatRuleContext context)
        {
            // set starting date to 59 minutes ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, 0, -59, 0, 0));
            var toDate = Repository.CurrentDate;

            // read hourly posting quota configuration parameter from web.config
            int hpqp = Settings.HourlyCommentPostingQuota;

            using (var db = new voatEntities())
            {
                // check how many comments user made in the last 59 minutes
                var userCommentSubmissionsInPastHour = db.Comments.Count(
                    m => m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (hpqp <= userCommentSubmissionsInPastHour)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his hourly posting quota for a given subverse
        protected bool UserHourlyPostingQuotaForSubUsed(VoatRuleContext context, string subverse)
        {
            // set starting date to 1 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -1, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.HourlyPostingQuotaPerSub;

            using (var db = new voatEntities())
            {
                // check how many submission user made in the last hour
                var userSubmissionsToTargetSub = db.Submissions.Count(
                    m => m.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        && m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= userSubmissionsToTargetSub)
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

            // set starting date to 1 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -1, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.HourlyGlobalPostingQuota;

            using (var db = new voatEntities())
            {
                // check how many submission user made in the last hour
                var totalUserSubmissionsForTimeSpam = db.Submissions.Count(m => m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase) && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= totalUserSubmissionsForTimeSpam)
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

            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily global posting quota configuration parameter from web.config
            int dpqps = Settings.DailyGlobalPostingQuota;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = db.Submissions.Count(m => m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase) && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= userSubmissionsToTargetSub)
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
            int dailyCrossPostQuota = Settings.DailyCrossPostingQuota;

            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            using (var db = new voatEntities())
            {
                var numberOfTimesSubmitted = db.Submissions
                    .Where(m => m.Content.Equals(url, StringComparison.OrdinalIgnoreCase)
                    && m.UserName.Equals(context.UserName, StringComparison.OrdinalIgnoreCase)
                    && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                int nrtimessubmitted = numberOfTimesSubmitted.Count();

                if (dailyCrossPostQuota <= nrtimessubmitted)
                {
                    return true;
                }
                return false;
            }
        }

        #endregion
    }
}
