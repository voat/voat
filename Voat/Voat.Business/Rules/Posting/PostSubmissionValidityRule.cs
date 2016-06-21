using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    //Ensure user isn't submitting links as titles
                    if (userSubmission.Title.Equals(userSubmission.Url, StringComparison.InvariantCultureIgnoreCase) || userSubmission.Url.Contains(userSubmission.Title))
                    {
                        return CreateOutcome(RuleResult.Denied, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");
                    }
                    // make sure the input URI is valid
                    if (!UrlUtility.IsUriValid(userSubmission.Url))
                    {
                        return CreateOutcome(RuleResult.Denied, "The url you are trying to submit is invalid");
                    }
                    break;
                case SubmissionType.Text:
                    break;
            }

            if (String.IsNullOrEmpty(userSubmission.Title))
            {
                return CreateOutcome(RuleResult.Denied, "A text submission must include a title");
            }
            if (userSubmission.Title.Length < 5)
            {
                return CreateOutcome(RuleResult.Denied, "A title may not be less than 5 characters");
            }
            if (Submissions.ContainsUnicode(userSubmission.Title))
            {
                return CreateOutcome(RuleResult.Denied,  "Submission title can not contain Unicode characters");
            }

            //if context.Subverse is null this means that it can't be found/doesn't exist
            if (context.Subverse == null || userSubmission.Subverse.Equals("all", StringComparison.OrdinalIgnoreCase)) //<-- the all subverse actually exists? HA! (Putts: leaving this code in because it's rad)
            {
                return CreateOutcome(RuleResult.Denied, "Subverse does not exist");
            }


            //// LINK TYPE SUBMISSION
            //if (submissionModel.Type == 2)
            //{
            //    // strip unicode if title contains unicode
            //    if (ContainsUnicode(submissionModel.LinkDescription))
            //    {
            //        submissionModel.LinkDescription = StripUnicode(submissionModel.LinkDescription);
            //    }

            //    // reject if title is whitespace or < than 5 characters
            //    if (submissionModel.LinkDescription.Length < 5 || String.IsNullOrWhiteSpace(submissionModel.LinkDescription))
            //    {
            //        return ("The title may not be less than 5 characters.");
            //    }

            //    // make sure the input URI is valid
            //    if (!UrlUtility.IsUriValid(submissionModel.Content))
            //    {
            //        // ABORT
            //        return ("The URI you are trying to submit is invalid.");
            //    }

            //    // check if target subvere allows submissions from globally banned hostnames
            //    if (!targetSubverse.ExcludeSitewideBans)
            //    {
            //        // check if hostname is banned before accepting submission
            //        var domain = UrlUtility.GetDomainFromUri(submissionModel.Content);
            //        if (BanningUtility.IsDomainBanned(domain))
            //        {
            //            // ABORT
            //            return ("The domain you are trying to submit is banned.");
            //        }
            //    }

            //    // check if user has reached daily crossposting quota
            //    if (UserHelper.DailyCrossPostingQuotaUsed(userName, submissionModel.Content))
            //    {
            //        // ABORT
            //        return ("You have reached your daily crossposting quota for this URL.");
            //    }

            //    // check if target subverse has thumbnails setting enabled before generating a thumbnail
            //    if (targetSubverse.IsThumbnailEnabled)
            //    {
            //        // try to generate and assign a thumbnail to submission model
            //        submissionModel.Thumbnail = await ThumbGenerator.ThumbnailFromSubmissionModel(submissionModel);
            //    }

            //    // flag the submission as anonymized if it was submitted to a subverse with active anonymized_mode
            //    submissionModel.IsAnonymized = targetSubverse.IsAnonymized;
            //    submissionModel.UserName = userName;
            //    submissionModel.Subverse = targetSubverse.Name;
            //    submissionModel.UpCount = 1;
            //    db.Submissions.Add(submissionModel);

            //    await db.SaveChangesAsync();
            //}
            //else
            //// MESSAGE TYPE SUBMISSION
            //{
            //    // strip unicode if submission contains unicode
            //    if (ContainsUnicode(submissionModel.Title))
            //    {
            //        submissionModel.Title = StripUnicode(submissionModel.Title);
            //    }

            //    // reject if title is whitespace or less than 5 characters
            //    if (submissionModel.Title.Length < 5 || String.IsNullOrWhiteSpace(submissionModel.Title))
            //    {
            //        return ("Sorry, submission title may not be less than 5 characters.");
            //    }

            //    // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
            //    submissionModel.IsAnonymized = targetSubverse.IsAnonymized;
            //    submissionModel.UserName = userName;
            //    submissionModel.Subverse = targetSubverse.Name;
            //    submissionModel.CreationDate = Repository.CurrentDate;
            //    submissionModel.UpCount = 1;
            //    db.Submissions.Add(submissionModel);

            //    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
            //    {
            //        submissionModel.Content = ContentProcessor.Instance.Process(submissionModel.Content, ProcessingStage.InboundPreSave, submissionModel);
            //    }

            //    await db.SaveChangesAsync();

            //    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
            //    {
            //        ContentProcessor.Instance.Process(submissionModel.Content, ProcessingStage.InboundPostSave, submissionModel);
            //    }
            //}


            return base.EvaluateRule(context);
        }
    }
   
}
