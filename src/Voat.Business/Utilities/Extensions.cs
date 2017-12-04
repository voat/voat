#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Voat.Common;
using Voat.Configuration;
using Voat.Domain.Models;

namespace Voat
{
    public static class Extensions
    {
        public static string DebugMessage(this Domain.Command.CommandResponse response)
        {
            if (VoatSettings.Instance.IsDevelopment)
            {
                if (response.Exception != null)
                {
                    return response.Exception.ToString();
                }
            }
            return response.Message;
        }
        public static string ToYesNo(this bool value)
        {
            return value ? "Yes" : "No";
        }
        public static string ToYesNo(this bool? value, string nullValue)
        {
            if (value.HasValue)
            {
                return value.Value.ToYesNo();
            }
            return nullValue;
        }

        public static string BasePath(this Domain.Models.DomainReference domainReference, Domain.Models.SortAlgorithm? sort = null)
        {
            string path = "";
            if (domainReference != null)
            {
                switch (domainReference.Type)
                {
                    case Domain.Models.DomainType.Subverse:
                        path = String.Format("/v/{0}/{1}", domainReference.Name, sort == null ? "" : sort.Value.ToString().ToLower());
                        break;
                    case Domain.Models.DomainType.Set:
                        path = String.Format("{0}/{1}", Utilities.VoatUrlFormatter.BasePath(domainReference), sort == null ? "" : sort.Value.ToString().ToLower());
                        break;
                    case Domain.Models.DomainType.User:
                        path = String.Format("/u/{0}", domainReference.Name);
                        break;
                }
            }
            return path.TrimEnd('/');
        }


        //public Dictionary<int, string> GetEnumValues(Type type)
        //{
        //    var dict = new Dictionary<int, string>();
        //}
        public static string ToChatTimeDisplay(this DateTime dateTime) 
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            return dateTime.ToString("hh:mm:ss", ci) + " UTC";
  
        }

        /// <summary>
        /// Returns a range based on the span provided. Does not standardize range, simply subtracts offset.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sortSpan"></param>
        /// <returns></returns>
        //This code needs to be refactored to use Voat.Common.DateRange 
        public static Tuple<DateTime, DateTime> ToRelativeRange(this DateTime dateTime, SortSpan sortSpan, SortDirection sortDirection = SortDirection.Reverse)
        {
            DateTime start = dateTime;
            DateTime end = dateTime;
            var directionMultiplier = sortDirection == SortDirection.Reverse ? -1 : 1;
            switch (sortSpan)
            {
                case SortSpan.Hour:
                    end = start.ToEndOfHour();
                    start = end.AddHours(1 * directionMultiplier);
                    break;

                case SortSpan.Day:
                    end = start.ToEndOfHour();
                    start = end.AddHours(24 * directionMultiplier);
                    break;

                case SortSpan.Week:
                    end = start.ToEndOfDay();
                    start = end.AddDays(7 * directionMultiplier);
                    break;

                case SortSpan.Month:
                    end = start.ToEndOfDay();
                    start = end.AddDays(30 * directionMultiplier);
                    break;

                case SortSpan.Quarter:
                    end = start.ToEndOfDay();
                    start = end.AddDays(90 * directionMultiplier);
                    break;

                case SortSpan.Year:
                    end = start.ToEndOfDay();
                    start = end.AddDays(365 * directionMultiplier);
                    break;

                default:
                case SortSpan.All:

                    //Date Range shouldn't be processed for this span
                    break;
            }

            return new Tuple<DateTime, DateTime>(start, end);
        }

        /// <summary>
        /// The purpose of this function is to standardize inputs so that we can cache ranged queries. Currently ranges
        /// use the current date which contains diffrent minute, second, and ms with each call. This function converts to common
        /// start and end ranges (beginning and ending of days, hours, etc.)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sortSpan"></param>
        /// <returns></returns>

        //This code needs to be refactored to use Voat.Common.DateRange 
        public static Tuple<DateTime, DateTime> ToRange(this DateTime dateTime, SortSpan sortSpan)
        {
            DateTime start = dateTime;
            DateTime end = dateTime;
            switch (sortSpan)
            {
                case SortSpan.Hour:
                    start = start.ToStartOfHour();
                    end = start.Add(TimeSpan.FromHours(-1));
                    break;

                case SortSpan.Day:
                    start = start.ToStartOfDay();
                    end = start.ToEndOfDay();
                    break;

                case SortSpan.Week:
                    start = start.ToStartOfWeek();
                    end = start.ToEndOfWeek();
                    break;

                case SortSpan.Month:
                    start = start.ToStartOfMonth();
                    end = start.ToEndOfMonth();
                    break;

                case SortSpan.Quarter:
                    var range = start.ToQuarterRange();
                    start = range.Item1;
                    end = range.Item2;
                    break;

                case SortSpan.Year:
                    start = start.ToStartOfYear();
                    end = start.ToEndOfYear();
                    break;

                default:
                case SortSpan.All:

                    //Date Range shouldn't be processed for this span
                    break;
            }

            return new Tuple<DateTime, DateTime>(start, end);
        }

        public static string ToJson(this object value, JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = new JsonSerializerSettings();
            }

            var content = JsonConvert.SerializeObject(value, settings);
            return content;
        }
    }
}
