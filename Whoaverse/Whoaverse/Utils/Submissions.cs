/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whoaverse.Utils
{
    public class Submissions
    {

        public static string CalcSubmissionAge(DateTime inPostingDateTime)
        {
            DateTime currentDateTime = new DateTime(Convert.ToInt32(DateTime.Now.Year), Convert.ToInt32(DateTime.Now.Month), Convert.ToInt32(DateTime.Now.Day), Convert.ToInt32(DateTime.Now.Hour), Convert.ToInt32(DateTime.Now.Minute), 00);
            TimeSpan duration = currentDateTime - inPostingDateTime;

            double totalHours = duration.TotalHours;

            if (totalHours > 24)
            {
                return Convert.ToInt32(duration.TotalDays) + " days";
            }
            else if (totalHours < 24 && totalHours > 1 || totalHours == 1)
            {
                return Convert.ToInt32(duration.TotalHours) + " hours";
            }
            else if (totalHours < 1)
            {
                return Convert.ToInt32(duration.TotalMinutes) + " minutes";
            }
            else
            {
                return "No idea when this was posted...";
            }
        }

        public static double CalcSubmissionAgeDouble(DateTime inPostingDateTime)
        {
            DateTime currentDateTime = new DateTime(Convert.ToInt32(DateTime.Now.Year), Convert.ToInt32(DateTime.Now.Month), Convert.ToInt32(DateTime.Now.Day), Convert.ToInt32(DateTime.Now.Hour), Convert.ToInt32(DateTime.Now.Minute), 00);
            TimeSpan duration = currentDateTime - inPostingDateTime;

            double totalHours = duration.TotalHours;

            if (totalHours > 24)
            {
                return duration.TotalDays;
            }
            else if (totalHours < 24 && totalHours > 1 || totalHours == 1)
            {
                return duration.TotalHours;
            }
            else if (totalHours < 1)
            {
                return duration.TotalMinutes;
            }
            else
            {
                return -10000;
            }
        }

    }
}