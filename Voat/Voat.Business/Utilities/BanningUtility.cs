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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Voat.Caching;
using Voat.Data.Models;
using Voat.Utilities.Components;

namespace Voat.Utilities
{
    public class BanningUtility
    {
        public static bool ContentContainsBannedDomain(string subverse, string contentToEvaluate)
        {
            if (!String.IsNullOrEmpty(contentToEvaluate))
            {
                //TODO: Change this to Query
                Subverse s = null;
                if (!String.IsNullOrEmpty(subverse))
                {
                    s = DataCache.Subverse.Retrieve(subverse);
                }
                if (s == null || (s != null && !s.ExcludeSitewideBans))
                {

                    MatchCollection matches = Regex.Matches(contentToEvaluate, CONSTANTS.HTTP_LINK_REGEX, RegexOptions.IgnoreCase);
                    List<string> domains = new List<string>();
                    foreach (Match match in matches)
                    {
                        string domain = match.Groups["domain"].Value;
                        string[] parts = domain.Split('.');
                        if (parts.Length > 2)
                        {
                            domain = String.Format("{0}.{1}", parts[parts.Length - 2], parts[parts.Length - 1]);
                        }
                        domains.Add(domain);
                    }
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

        public static bool IsDomainBanned(params string[] domains)
        {
            foreach (var domain in domains)
            {
                using (var db = new voatEntities())
                {
                    if (domain != null)
                    {
                        return db.BannedDomains.Any(r => r.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            return false;
        }
    }
}
