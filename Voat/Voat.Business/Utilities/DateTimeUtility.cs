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
    public static class DateTimeUtility
    {
        // a method to convert natural language to DateTime datatype
        public static DateTime DateRangeToDateTime(string daterange)
        {
            switch (daterange)
            {
                case "week":
                    // set start date to past 1 week
                    return DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0, 0));
                case "month":
                    // set start date to past 30 days
                    return DateTime.Now.Add(new TimeSpan(-30, 0, 0, 0, 0));
                case "year":
                    // set start date to past 365 days
                    return DateTime.Now.Add(new TimeSpan(-365, 0, 0, 0, 0));
                case "all":
                    // set start date to past 100 years
                    return DateTime.Now.AddYears(-100);
                default:
                    // set start date to past 24 hours
                    return DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));
            }
        }
    }
}
