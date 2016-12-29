using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Rules.Posting.Base;
using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission edit if data passes spam filters", "approved = (submission.Filters() == 0)")]
    public class PostSubmissionFilterRule : SubmissionFilterRule
    {
        public PostSubmissionFilterRule()
            : base("Submission Filter", "5.4.1", RuleScope.PostSubmission)
        {
        }
    }
}
