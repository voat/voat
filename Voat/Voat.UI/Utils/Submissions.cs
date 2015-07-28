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
using Voat.Models;

namespace Voat.Utils
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
        public static async Task<string> AddNewLinkSubmission(Message submissionModel, Subverse targetSubverse, string userName)
        {
            using (var db = new voatEntities())
            {
                // strip unicode if title contains unicode
                if (ContainsUnicode(submissionModel.Linkdescription))
                {
                    submissionModel.Linkdescription = StripUnicode(submissionModel.Linkdescription);
                }

                // abort if title is < than 10 characters
                if (submissionModel.Linkdescription.Length < 10)
                {
                    // ABORT
                    return ("The title may not be less than 10 characters.");
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
                if (User.DailyCrossPostingQuotaUsed(userName, submissionModel.MessageContent))
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
            
            // null is returned if no errors were raised
            return null;
        }
    }
}