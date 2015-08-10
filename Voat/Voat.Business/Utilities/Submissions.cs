/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Utilities.Components;

namespace Voat.Utilities
{
    public static class Submissions
    {
        private const double Tolerance = 0.01;

        // calculate submission age in days, hours or minutes for use in views
        public static string CalcSubmissionAge(DateTime inPostingDateTime)
        {
            var currentDateTime = DateTime.Now;
            var duration = currentDateTime - inPostingDateTime;
            return CalcSubmissionAge(duration);
        }

        private static string IsPlural(int amount)
        {
            return (amount == 1 ? "" : "s");
        }

        public static string CalcSubmissionAge(TimeSpan span)
        {

            string result = "No idea when this was posted...";

            if (span.TotalDays > 365)
            {
                //years
                double years = Math.Round(span.TotalDays / 365, 1);
                result = String.Format("{0} year{1}", years, (years > 1.0 ? "s" : ""));
            }
            else if (span.TotalDays > 31)
            {
                //months
                int months = (int)(span.TotalDays / 31);
                result = String.Format("{0} month{1}", months, IsPlural(months));
            }
            else if (span.TotalHours >= 24)
            {
                //days 
                result = String.Format("{0} day{1}", (int)span.TotalDays, IsPlural((int)span.TotalDays));
            }
            else if (span.TotalHours > 1 || Math.Abs(span.TotalHours - 1) < Tolerance)
            {
                //hours
                result = String.Format("{0} hour{1}", (int)span.TotalHours, IsPlural((int)span.TotalHours));
            }
            else if (span.TotalSeconds >= 60)
            {
                //minutes
                result = String.Format("{0} minute{1}", (int)span.TotalMinutes, IsPlural((int)span.TotalMinutes));
            }
            else
            {
                //seconds
                result = String.Format("{0} second{1}", (int)span.TotalSeconds, IsPlural((int)span.TotalSeconds));
            }

            return result;
        }

        // calculate submission age in hours from posting date for ranking purposes
        public static double CalcSubmissionAgeDouble(DateTime inPostingDateTime)
        {
            var currentDateTime = DateTime.Now;
            var duration = currentDateTime - inPostingDateTime;

            return duration.TotalHours;
        }

        // check if a string contains unicode characters
        public static bool ContainsUnicode(string stringToTest)
        {
            const int maxAnsiCode = 255;
            return stringToTest.Any(c => c > maxAnsiCode);
        }

        // string unicode characters from a string
        public static string StripUnicode(string stringToClean)
        {
            return Regex.Replace(stringToClean, @"[^\u0000-\u007F]", string.Empty);
        }

        // add new link submission
        public static async Task<string> AddNewSubmission(Message submissionModel, Subverse targetSubverse, string userName)
        {
            using (var db = new voatEntities())
            {
                // LINK TYPE SUBMISSION
                if (submissionModel.Type == 2)
                {
                    // strip unicode if title contains unicode
                    if (ContainsUnicode(submissionModel.Linkdescription))
                    {
                        submissionModel.Linkdescription = StripUnicode(submissionModel.Linkdescription);
                    }

                    // reject if title is whitespace or < than 5 characters
                    if (submissionModel.Linkdescription.Length < 5 || String.IsNullOrWhiteSpace(submissionModel.Linkdescription))
                    {
                        return ("The title may not be less than 5 characters.");
                    }

                    // make sure the input URI is valid
                    if (!UrlUtility.IsUriValid(submissionModel.MessageContent))
                    {
                        // ABORT
                        return ("The URI you are trying to submit is invalid.");
                    }

                    // check if target subvere allows submissions from globally banned hostnames
                    if (!targetSubverse.exclude_sitewide_bans)
                    {
                        // check if hostname is banned before accepting submission
                        var domain = UrlUtility.GetDomainFromUri(submissionModel.MessageContent);
                        if (BanningUtility.IsHostnameBanned(domain))
                        {
                            // ABORT
                            return ("The hostname you are trying to submit is banned.");
                        }
                    }

                    // check if user has reached daily crossposting quota
                    if (UserHelper.DailyCrossPostingQuotaUsed(userName, submissionModel.MessageContent))
                    {
                        // ABORT
                        return ("You have reached your daily crossposting quota for this URL.");
                    }

                    // check if target subverse has thumbnails setting enabled before generating a thumbnail
                    if (targetSubverse.enable_thumbnails)
                    {
                        // try to generate and assign a thumbnail to submission model
                        submissionModel.Thumbnail = await ThumbGenerator.ThumbnailFromSubmissionModel(submissionModel);
                    }

                    // flag the submission as anonymized if it was submitted to a subverse with active anonymized_mode
                    if (targetSubverse.anonymized_mode)
                    {
                        submissionModel.Anonymized = true;
                    }
                    else
                    {
                        submissionModel.Name = userName;
                    }

                    // accept submission and save it to the database
                    submissionModel.Subverse = targetSubverse.name;
                    submissionModel.Likes = 1;
                    db.Messages.Add(submissionModel);

                    // update last submission received date for target subverse
                    targetSubverse.last_submission_received = DateTime.Now;
                    await db.SaveChangesAsync();
                }
                else
                // MESSAGE TYPE SUBMISSION
                {
                    // strip unicode if submission contains unicode
                    if (ContainsUnicode(submissionModel.Title))
                    {
                        submissionModel.Title = StripUnicode(submissionModel.Title);
                    }

                    // reject if title is whitespace or less than 5 characters
                    if (submissionModel.Title.Length < 5 || String.IsNullOrWhiteSpace(submissionModel.Title))
                    {
                        return ("Sorry, submission title may not be less than 5 characters.");
                    }

                    // flag the submission as anonymized if it was submitted to a subverse with active anonymized_mode
                    if (targetSubverse.anonymized_mode)
                    {
                        submissionModel.Anonymized = true;
                    }
                    else
                    {
                        submissionModel.Name = userName;
                    }

                    // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
                    submissionModel.Subverse = targetSubverse.name;
                    submissionModel.Date = DateTime.Now;
                    submissionModel.Likes = 1;
                    db.Messages.Add(submissionModel);

                    // update last submission received date for target subverse
                    targetSubverse.last_submission_received = DateTime.Now;

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                    {
                        submissionModel.MessageContent = ContentProcessor.Instance.Process(submissionModel.MessageContent, ProcessingStage.InboundPreSave, submissionModel);
                    }

                    await db.SaveChangesAsync();

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                    {
                        ContentProcessor.Instance.Process(submissionModel.MessageContent, ProcessingStage.InboundPostSave, submissionModel);
                    }
                }
            }
            
            // null is returned if no errors were raised
            return null;
        }

        // various spam checks, to be replaced with new rule engine
        public static async Task<string> PreAddSubmissionCheck(Message submissionModel, HttpRequestBase request, string userName, Subverse targetSubverse, Func<HttpRequestBase, Task<bool>> captchaValidator)
        {
            // reject if user has reached global daily submission quota
            if (UserHelper.UserDailyGlobalPostingQuotaUsed(userName))
            {
                return ("You have reached your daily global submission quota.");
            }

            // reject if user has reached global hourly submission quota
            if (UserHelper.UserHourlyGlobalPostingQuotaUsed(userName))
            {
                return ("You have reached your hourly global submission quota.");
            }

            // TODO: reject if a submission with this title was posted in the last 60 minutes

            // check if user has reached hourly posting quota for target subverse
            if (UserHelper.UserHourlyPostingQuotaForSubUsed(userName, submissionModel.Subverse))
            {
                return ("You have reached your hourly submission quota for this subverse.");
            }

            // check if user has reached daily posting quota for target subverse
            if (UserHelper.UserDailyPostingQuotaForSubUsed(userName, submissionModel.Subverse))
            {
                return ("You have reached your daily submission quota for this subverse.");
            }

            // verify recaptcha if user has less than 25 CCP
            var userCcp = Karma.CommentKarma(userName);
            if (userCcp < 25)
            {
                bool isCaptchaCodeValid = await captchaValidator(request);

                if (!isCaptchaCodeValid)
                {
                    // TODO: SET PREVENT SPAM DELAY TO 0
                    return ("Incorrect recaptcha answer.");
                }
            }

            // if user CCP or SCP is less than -50, allow only X submissions per 24 hours
            var userScp = Karma.LinkKarma(userName);
            if (userCcp <= -50 || userScp <= -50)
            {
                var quotaUsed = UserHelper.UserDailyPostingQuotaForNegativeScoreUsed(userName);
                if (quotaUsed)
                {
                    return ("You have reached your daily submission quota. Your current quota is " + Settings.DailyPostingQuotaForNegativeScore + " submission(s) per 24 hours.");
                }
            }

            // check if subverse has "authorized_submitters_only" set and dissalow submission if user is not allowed submitter
            if (targetSubverse.authorized_submitters_only)
            {
                if (!UserHelper.IsUserSubverseModerator(userName, targetSubverse.name))
                {
                    return ("You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization.");
                }
            }

            // null is returned if all checks have passed
            return null;
        }
    }
}