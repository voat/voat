namespace Voat.Models.Quotas
{
    using System.Collections.Generic;

    public class CommentCountModel
    {
        public int LastDayTotalCommentCount { get; set; }

        public IEnumerable<string> Validate(Restrictions.NegativeScoreQuotas quotas)
        {
            if (LastDayTotalCommentCount > quotas.DailyCommentCount)
            {
                yield return
                    string.Format("You have reached your daily submission quota. Your current quota is {0}",
                        quotas.DailyCommentCount);
            }

        }
    }

    public class PostCountModel
    {
        public int DailyCrossPostCount { get; set; }
        public int LastDaySubPostCount { get; set; }
        public int LastHourSubPostCount { get; set; }
        public int LastDayTotalPostCount { get; set; }

        public IEnumerable<string> Validate(Restrictions.RegularQuotas quotas)
        {
            if (DailyCrossPostCount > quotas.DailyCrossPosting)
            {
                yield return "You have reached your daily crossposting quota for this URL.";
            }

            if (LastHourSubPostCount > quotas.HourlyPostCountPerSub)
            {
                yield return "You have reached your hourly submission quota for this subverse.";
            }

            if (LastDaySubPostCount > quotas.DailyPostCountPerSub)
            {
                yield return "You have reached your daily submission quota for this subverse.";
            }
        }

        public IEnumerable<string> Validate(Restrictions.NegativeScoreQuotas quotas)
        {
            if (LastDayTotalPostCount > quotas.DailyPostingCount)
            {
                yield return
                    string.Format("You have reached your daily submission quota. Your current quota is {0}",
                        quotas.DailyPostingCount);
            }
        }

       
    }
}