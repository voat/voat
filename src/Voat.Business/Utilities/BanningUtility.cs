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
using System.Text.RegularExpressions;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public class BanningUtility
    {
        public static bool ContentContainsBannedDomain(string subverse, string contentToEvaluate)
        {
            if (!String.IsNullOrEmpty(contentToEvaluate))
            {
                Subverse s = null;
                if (!String.IsNullOrEmpty(subverse))
                {
                    s = DataCache.Subverse.Retrieve(subverse);
                }
                if (s == null || (s != null && !s.ExcludeSitewideBans))
                {
                    List<string> domains = new List<string>();
                    ProcessMatches(Regex.Matches(contentToEvaluate, CONSTANTS.PROTOCOL_LESS_LINK_REGEX, RegexOptions.IgnoreCase), domains);
                    if (domains.Count > 0)
                    {
                        bool hasBannedDomainLinks = BanningUtility.IsDomainBanned(domains.ToArray());
                        if (hasBannedDomainLinks)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void ProcessMatches(MatchCollection matches, List<string> domains)
        {
            foreach (Match match in matches)
            {
                string domain = match.Groups["domain"].Value.ToLower();
                if (!String.IsNullOrWhiteSpace(domain) && !domains.Contains(domain))
                {
                    domains.Add(domain);
                }

                var queryGroup = match.Groups["query"];
                if (queryGroup != null && !String.IsNullOrEmpty(queryGroup.Value))
                {
                    ProcessMatches(Regex.Matches(queryGroup.Value, CONSTANTS.HTTP_LINK_REGEX, RegexOptions.IgnoreCase), domains);
                }
            }
        }

        public static bool IsDomainBanned(params string[] domains)
        {
            using (var repo = new Repository())
            {
                var bannedDomains = repo.BannedDomains(domains);
                return bannedDomains.Any();
            }
        }
    }
}
