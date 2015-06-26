namespace Voat.Models.Quotas
{
    using System.Collections.Specialized;

    public class Restrictions
    {
        public class RegularQuotas
        {
            public int DailyPostCountPerSub { get; set; }
            public int HourlyPostCountPerSub { get; set; }
            public int DailyCrossPosting { get; set; }    
        }

        public class NegativeScoreQuotas
        {
            public int DailyCommentCount { get; set; }
            public int DailyPostingCount { get; set; }    
        }

        public RegularQuotas Regular { get; private set; }
        public NegativeScoreQuotas NegativeScore { get; private set; }

        public Restrictions(RegularQuotas regularQuotas, NegativeScoreQuotas negativeScoreQuotas)
        {
            Regular = regularQuotas;
            NegativeScore = negativeScoreQuotas;
        }

        private Restrictions()
        {
        }

        public static Restrictions FromConfig(NameValueCollection config)
        {
            return new Restrictions
            {
                Regular = new RegularQuotas
                {
                    DailyCrossPosting = int.Parse(config["dailyCrossPostingQuota"]),
                    HourlyPostCountPerSub = int.Parse(config["hourlyPostingQuotaPerSub"]),
                    DailyPostCountPerSub = int.Parse(config["dailyPostingQuotaPerSub"])
                },
                NegativeScore = new NegativeScoreQuotas
                {
                    DailyCommentCount = int.Parse(config["dailyCommentPostingQuotaForNegativeScore"]),
                    DailyPostingCount = int.Parse(config["dailyPostingQuotaForNegativeScore"]),    
                }
            };
        }
    }
}