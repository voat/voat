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
    public class SubmissionFilterRule : VoatRule
    {
        public SubmissionFilterRule(string name, string number, RuleScope scope)
            : base(name, number, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            UserSubmission submission = context.PropertyBag.UserSubmission;

            //Check if content matches spam filters
            IEnumerable<FilterMatch> result = null;
            switch (submission.Type)
            {
                case SubmissionType.Link:
                    result = FilterUtility.Match(String.Concat(submission.Title, " ", submission.Url));
                    break;

                case SubmissionType.Text:
                    result = FilterUtility.Match(String.Concat(submission.Title, " ", submission.Content));
                    break;
            }
            if (result.Any())
            {
                return CreateOutcome(RuleResult.Denied, $"Submission does not pass filter: {result.First().Filter.Name}");
            }

            return Allowed;
        }
    }
}
