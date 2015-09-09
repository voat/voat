/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System.Data.Entity;
using System.Linq;
using Voat.Data.Models;
using Voat.Models;

namespace Voat.UI.Utilities
{
    //REFACTOR: Duplicate logic
    public static class SetsUtility
    {
        public static IQueryable<SetSubmission> TopRankedSubmissionsFromASub(string subverseName, DbSet<Submission> submissionDBSet, string setName, int desiredResults, int? skip)
        {
            int recordsToSkip = (skip ?? 0);

            // skip could be used here
            var topRankedSubmissions = (from submission in submissionDBSet
                                        where !submission.IsDeleted && submission.Subverse == subverseName
                                        select new SetSubmission
                                        {
                                            ID = submission.ID,
                                            Votes = submission.Votes,
                                            UserName = submission.UserName,
                                            CreationDate = submission.CreationDate,
                                            Type = submission.Type,
                                            LinkDescription = submission.LinkDescription,
                                            Title = submission.Title,
                                            Rank = submission.Rank,
                                            Content = submission.Content,
                                            Subverse = submission.Subverse,
                                            UpCount = submission.UpCount,
                                            DownCount = submission.DownCount,
                                            Thumbnail = submission.Thumbnail,
                                            LastEditDate = submission.LastEditDate,
                                            FlairLabel = submission.FlairLabel,
                                            FlairCss = submission.FlairCss,
                                            IsAnonymized = submission.IsAnonymized,
                                            Views = submission.Views,
                                            Comments = submission.Comments,
                                            SubmissionVoteTrackers = submission.SubmissionVoteTrackers,
                                            Subverse1 = submission.Subverse1,
                                            StickiedSubmission = submission.StickiedSubmission,
                                            ViewStatistics = submission.ViewStatistics,
                                            ParentSet = setName
                                        }).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).Skip(recordsToSkip).Take(desiredResults).AsNoTracking();

            return topRankedSubmissions;
        }

        public static IQueryable<SetSubmission> NewestSubmissionsFromASub(string subverseName, DbSet<Submission> submissionDBSet, string setName, int desiredResults)
        {
            var topRankedSubmissions = (from submission in submissionDBSet
                                        where !submission.IsDeleted && submission.Subverse == subverseName
                                        select new SetSubmission
                                        {
                                            ID = submission.ID,
                                            Votes = submission.Votes,
                                            UserName = submission.UserName,
                                            CreationDate = submission.CreationDate,
                                            Type = submission.Type,
                                            LinkDescription = submission.LinkDescription,
                                            Title = submission.Title,
                                            Rank = submission.Rank,
                                            Content = submission.Content,
                                            Subverse = submission.Subverse,
                                            UpCount = submission.UpCount,
                                            DownCount = submission.DownCount,
                                            Thumbnail = submission.Thumbnail,
                                            LastEditDate = submission.LastEditDate,
                                            FlairLabel = submission.FlairLabel,
                                            FlairCss = submission.FlairCss,
                                            IsAnonymized = submission.IsAnonymized,
                                            Views = submission.Views,
                                            Comments = submission.Comments,
                                            SubmissionVoteTrackers = submission.SubmissionVoteTrackers,
                                            Subverse1 = submission.Subverse1,
                                            StickiedSubmission = submission.StickiedSubmission,
                                            ViewStatistics = submission.ViewStatistics,
                                            ParentSet = setName
                                        }).OrderByDescending(s => s.CreationDate).Take(desiredResults).AsNoTracking();

            return topRankedSubmissions;
        }
    }
}