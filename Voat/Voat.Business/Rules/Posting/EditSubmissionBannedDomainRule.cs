using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.Rules.Posting.Base;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission edit if data does not contain banned domains.", "approved = (submission.ContainsBannedDomains() == false)")]
    public class EditSubmissionBannedDomainRule : SubmissionBannedDomainRule
    {
        public EditSubmissionBannedDomainRule()
            : base("Submission Banned Domain", "5.2.2", RuleScope.EditSubmission)
        {
        }
    }
}
