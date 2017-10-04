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
using Voat.Common;
using Voat.Configuration;
using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission if it doesn't contain invalid data.", "approved = (submission.IsValid == true)")]
    public class PostSubmissionValidityRule : VoatRule
    {
        public PostSubmissionValidityRule()
            : base("Submission Validity", "5.5", RuleScope.PostSubmission)
        {
            this.Order = 1; //we want this rule to run first to check basic data
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            UserSubmission userSubmission = context.PropertyBag.UserSubmission;

            if (userSubmission == null)
            {
                return CreateOutcome(RuleResult.Denied, "The submission must not be null");
            }
            if (String.IsNullOrEmpty(userSubmission.Subverse))
            {
                return CreateOutcome(RuleResult.Denied, "A subverse must be provided");
            }

            switch (userSubmission.Type)
            {
                case SubmissionType.Link:
                    if (String.IsNullOrEmpty(userSubmission.Url))
                    {
                        return CreateOutcome(RuleResult.Denied, "A link submission must include a url");
                    }

                    // make sure the input URI is valid
                    if (!UrlUtility.IsUriValid(userSubmission.Url))
                    {
                        return CreateOutcome(RuleResult.Denied, "The url you are trying to submit is invalid");
                    }

                    //Ensure user isn't submitting links as titles
                    var linkDomain = UrlUtility.GetDomainFromUri(userSubmission.Url);
                    if (userSubmission.Title.Equals(userSubmission.Url, StringComparison.InvariantCultureIgnoreCase) || userSubmission.Title.Contains(linkDomain))
                    {
                        return CreateOutcome(RuleResult.Denied, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");
                    }

                    break;

                case SubmissionType.Text:
                    break;
            }

           
            if (String.IsNullOrEmpty(userSubmission.Title))
            {
                return CreateOutcome(RuleResult.Denied, "A text submission must include a title");
            }
            if (!VoatSettings.Instance.AllowUnicodeInTitles && userSubmission.Title.ContainsUnicode())
            {
                return CreateOutcome(RuleResult.Denied, "Submission title can not contain Unicode or unprintable characters");
            }
            int minTitleLength = VoatSettings.Instance.MinimumTitleLength;
            int maxTitleLength = VoatSettings.Instance.MaximumTitleLength;

            if (userSubmission.Title.Length < minTitleLength || userSubmission.Title.Length > maxTitleLength)
            {
                return CreateOutcome(RuleResult.Denied, $"A title must be between {minTitleLength} and {maxTitleLength} characters");
            }
            // make sure the title isn't a url
            if (UrlUtility.IsUriValid(userSubmission.Title))
            {
                return CreateOutcome(RuleResult.Denied, "Submission title is a url? Why would you even think about doing this?! Why?");
            }
            //if context.Subverse is null this means that it can't be found/doesn't exist
            if (context.Subverse == null || userSubmission.Subverse.IsEqual("all")) //<-- the all subverse actually exists? HA! (Putts: leaving this code in because it's rad)
            {
                return CreateOutcome(RuleResult.Denied, "Subverse does not exist");
            }

            //if (context.Subverse.IsAdminDisabled.HasValue && context.Subverse.IsAdminDisabled.Value)
            //{
            //    return CreateOutcome(RuleResult.Denied, "Submissions to disabled subverses are not allowed");
            //}

            return base.EvaluateRule(context);
        }
    }
}
