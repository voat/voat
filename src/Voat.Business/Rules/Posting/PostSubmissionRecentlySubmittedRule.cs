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
using Voat.Data;
using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission if the url hasn't been recently submitted", "approved = (submission.Exists(duration: 15 days) == false)")]
    public class PostSubmissionRecentlySubmittedRule : VoatRule
    {
        public PostSubmissionRecentlySubmittedRule()
            : base("Recently Submitted", "5.7", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            UserSubmission submission = context.PropertyBag.UserSubmission;
            switch (submission.Type)
            {
                case SubmissionType.Link:
                    Data.Models.Submission recentlySubmitted = null;
                    using (var repo = new Repository())
                    {
                        recentlySubmitted = repo.FindSubverseLinkSubmission(context.Subverse.Name, submission.Url, TimeSpan.FromDays(15));
                    }
                    if (recentlySubmitted != null)
                    {
                        string url = VoatUrlFormatter.BuildUrlPath(null, new Common.PathOptions() { FullyQualified = true, ProvideProtocol = true }, $"v/{recentlySubmitted.Subverse}/{recentlySubmitted.ID}");

                        return CreateOutcome(RuleResult.Denied, $"Sorry, this link has already been submitted recently. {url}");
                    }
                    break;

                case SubmissionType.Text:

                    //containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, submission.Content);
                    break;
            }

            return Allowed;
        }
    }
}
