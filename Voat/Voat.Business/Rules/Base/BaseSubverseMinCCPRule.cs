using System;
using System.Collections.Generic;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    public abstract class BaseSubverseMinimumCCPRule : VoatRule
    {
        public BaseSubverseMinimumCCPRule(string name, string number, RuleScope scope)
            : base(name, number, scope)
        {
        }

        public override IDictionary<string, Type> RequiredContext
        {
            get
            {
                return new Dictionary<string, Type>() { { "Subverse", typeof(Subverse) } };
            }
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var subverse = context.Subverse;

            int? subMinCCP = subverse.MinCCPForDownvote;

            if (subMinCCP.HasValue && subMinCCP.Value > 0)
            {
                var q = new QueryUserContributionPoints(Domain.Models.ContentType.Comment, subverse.Name);
                var score = q.Execute();

                if (score.Sum < subMinCCP.Value)
                {
                    return CreateOutcome(RuleResult.Denied, String.Format("Subverse '{0}' requires {1}CCP to downvote", subverse.Name, subMinCCP.Value.ToString()));
                }
            }

            return base.EvaluateRule(context);
        }
    }
}
