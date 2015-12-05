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
    //Simple object to store highest rank because we need to know submission ID to update it in cache
    public class HighestRank
    {
        public double Rank { get; set; }
        public int SubmissionID { get; set; }
    }

    public static class Ranking
    {
        private static string GetHighestRankingCacheKey(string subverse)
        {
            return String.Format("subverse.{0}.highest-rank", subverse).ToLower();
        }
        public static HighestRank GetSubverseHighestRanking(string subverse)
        {
            var highestRank =  CacheHandler.Register(GetHighestRankingCacheKey(subverse), new Func<HighestRank>(() => {
                using (var db = new voatEntities())
                {
                    var submission = db.Submissions.OrderByDescending(x => x.Rank).Where(x => x.Subverse == subverse).FirstOrDefault();
                    return new HighestRank() { Rank = submission.Rank, SubmissionID = submission.ID };
                }
            }), TimeSpan.FromMinutes(30));

            return highestRank;

        }
        public static void UpdateSubverseHighestRanking(string subverse, int submissionID, double newRank)
        {
            var highestRankCacheEntry = GetSubverseHighestRanking(subverse);

            if (highestRankCacheEntry != null)
            {
                if (highestRankCacheEntry.Rank < newRank)
                {
                    if (highestRankCacheEntry.SubmissionID != submissionID)
                    {
                        highestRankCacheEntry.SubmissionID = submissionID;
                    }

                    highestRankCacheEntry.Rank = newRank;

                    CacheHandler.Replace(GetHighestRankingCacheKey(subverse), new Func<HighestRank, HighestRank>(current => highestRankCacheEntry));
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

        public static double CalculateNewRelativeRank(double rank, double highestRankInSubverse)
        {
            double relativeRank = rank / highestRankInSubverse;

            return double.IsNaN(relativeRank) ? 0 : Math.Round(relativeRank, 7);
        }
    }
}