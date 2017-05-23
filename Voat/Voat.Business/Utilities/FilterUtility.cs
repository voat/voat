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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Query;

namespace Voat.Utilities
{
    public class FilterMatch
    {
        public Filter Filter { get; set; }
        public Group Group { get; set; }
    }

    public static class FilterUtility
    {
        public static IEnumerable<FilterMatch> Match(params string[] content)
        {
            var concatContent = (content == null ? null : String.Join(" ", content.Where(x => !String.IsNullOrEmpty(x))));
            return Match(concatContent);
        }

        public static IEnumerable<FilterMatch> Match(string content, bool matchAll = false)
        {
            List<FilterMatch> result = new List<FilterMatch>();
            var filters = Filters;
            //only execute if we have active filters.
            if (filters.Any())
            {
                string regexString = String.Format("{0}", String.Join("|", filters.Select(x => $"(?<F{x.ID.ToString()}>{x.Pattern})")));
                var regex = new Regex(regexString, RegexOptions.IgnoreCase);

                var matches = regex.MatchNamedGroup(content, matchAll);

                foreach (var keyPair in matches)
                {
                    if (keyPair.Key[0] == 'F')
                    {
                        result.Add(new FilterMatch() {
                            Filter = filters.First(x => x.ID == int.Parse(keyPair.Key.Substring(1))),
                            Group = keyPair.Value
                        });
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, Group> MatchNamedGroup(this Regex regex, string input, bool matchAll = false)
        {
            var result = new Dictionary<string, Group>();

            if (!String.IsNullOrEmpty(input))
            {
                var match = regex.Match(input);

                string[] groupNames = regex.GetGroupNames();

                ProcessMatch(match, groupNames, result);

                if (matchAll)
                {
                    do
                    {
                        match = match.NextMatch();
                        ProcessMatch(match, groupNames, result);
                    } while (match.Success);
                }
            }

            return result;
        }
        private static IEnumerable<Filter> Filters
        {
            get
            {
                var q = new QueryFilters();
                return q.Execute();
            }
        }

        private static void ProcessMatch(Match match, string[] groupNames, Dictionary<string, Group> result)
        {
            if (match.Success)
            {
                GroupCollection groups = match.Groups;
                foreach (string groupName in groupNames)
                {
                    if (groups[groupName].Captures.Count > 0)
                    {
                        if (!result.ContainsKey(groupName))
                        {
                            result.Add(groupName, groups[groupName]);
                        }
                    }
                }
            }
        }
    }
}
