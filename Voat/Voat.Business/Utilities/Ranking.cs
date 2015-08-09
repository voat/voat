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

namespace Voat.Utilities
{
    public static class Ranking
    {
        // re-rank a submission
        public static double CalculateNewRank(double currentRank, double submissionAge, double score)
        {
            const double penalty = 1;

            var newRank = (Math.Pow((score - 1), 0.8)) / (Math.Pow((submissionAge + 2), 1.8)) * penalty;

            return double.IsNaN(newRank) ? 0 : Math.Round(newRank, 7);
        }
    }
}