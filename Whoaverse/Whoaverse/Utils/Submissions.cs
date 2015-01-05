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

            var totalHours = duration.TotalHours;

            if (totalHours > 24)
            {
                return Convert.ToInt32(duration.TotalDays) + " days";
            }
            if (totalHours < 24 && totalHours > 1 || Math.Abs(totalHours - 1) < Tolerance)
            {
                return Convert.ToInt32(duration.TotalHours) + " hours";
            }
            if (totalHours < 1)
            {
                return Convert.ToInt32(duration.TotalMinutes) + " minutes";
            }
            return "No idea when this was posted...";
        }

        // calculate submission age in hours from posting date for ranking purposes
        public static double CalcSubmissionAgeDouble(DateTime inPostingDateTime)
        {
            var currentDateTime = DateTime.Now;
            var duration = currentDateTime - inPostingDateTime;

            return duration.TotalHours;            
        }
        
    }
}