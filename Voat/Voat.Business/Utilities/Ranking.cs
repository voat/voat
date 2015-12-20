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

using System;
using Voat.Data.Models;
using System.Linq;

namespace Voat.Utilities
{
    public static class Ranking
    {
        private static string GetHighestRankingCacheKey(string subverse)
        {
            return String.Format("subverse.{0}.highest-rank", subverse).ToLower();
        }
        public static double? GetSubverseHighestRanking(string subverse)
        {
            var highestRank =  CacheHandler.Register(GetHighestRankingCacheKey(subverse), new Func<double?>(() => {
                using (var db = new voatEntities())
                {
                    var submission = db.Submissions.OrderByDescending(x => x.Rank).Where(x => x.Subverse == subverse).FirstOrDefault();
                    if (submission != null)
                    {
                        return submission.Rank;
                    }
                    return null;
                }
            }), TimeSpan.FromMinutes(30));

            return highestRank;

        }
        public static void UpdateSubverseHighestRanking(string subverse, double newRank)
        {
            var highestRankCacheEntry = GetSubverseHighestRanking(subverse);

            if (highestRankCacheEntry != null)
            {
                if (highestRankCacheEntry < newRank)
                {
                    CacheHandler.Replace(GetHighestRankingCacheKey(subverse), new Func<double?, double?>(current => highestRankCacheEntry));
                }
            }
        }

        // re-rank a submission
        public static double CalculateNewRank(double currentRank, double submissionAge, double score)
        {
            const double penalty = 1;

            var newRank = (Math.Pow((score - 1), 0.8)) / (Math.Pow((submissionAge + 2), 1.8)) * penalty;

            return double.IsNaN(newRank) ? 0 : Math.Round(newRank, 7);
        }

        public static double? CalculateNewRelativeRank(double rank, double? highestRankInSubverse)
        {
            //Handle zero highest rank, return current rank.
            var d = rank / (highestRankInSubverse == 0.0 ? 1 : highestRankInSubverse);
            if (d != null)
            {
                double relativeRank = (double) d;

                return double.IsNaN(relativeRank) ? 0 : Math.Round(relativeRank, 7);
            }
            return null;
        }

        public static void RerankSubmission(Submission submission)
        {
            double currentScore = submission.UpCount - submission.DownCount;
            double submissionAge = Submissions.CalcSubmissionAgeDouble(submission.CreationDate);
            double newRank = CalculateNewRank(submission.Rank, submissionAge, currentScore);

            submission.Rank = newRank;

            // calculate relative rank
            var subCtr = GetSubverseHighestRanking(submission.Subverse);
            var relRank = CalculateNewRelativeRank(newRank, subCtr);
            if (relRank != null)
            {
                submission.RelativeRank = relRank.Value;
            }

            //update cache if higher rank that what's in there already
            UpdateSubverseHighestRanking(submission.Subverse, newRank);
        }
    }
}