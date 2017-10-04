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

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Voat.Common;
using Voat.Configuration;
using Voat.Domain.Models;

namespace Voat.Utilities
{
    /// <summary>
    /// This utility can resolve image paths for Voat. The API benifits from qualified urls and the MVC UI benifits from partials which this utility supports
    /// </summary>
    public static class VoatUrlFormatter
    {
        public static string Subverse(string subverse, PathOptions options = null)
        {
            //these are all default relative I believe
            options = options == null ? new PathOptions(false, false) : options;

            if (!String.IsNullOrEmpty(subverse))
            {
                return BuildUrlPath(null, options, "v", subverse);
                //return $"{GetProtocol(protocol)}//{_domain}/v/{subverse}";
            }
            return "#";
        }
        public static string Set(string setName, PathOptions options = null)
        {
            //these are all default relative I believe
            options = options == null ? new PathOptions(false, false) : options;

            if (!String.IsNullOrEmpty(setName))
            {
                return BuildUrlPath(null, options, "s", setName);
                //return $"{GetProtocol(protocol)}//{_domain}/v/{subverse}";
            }
            return "#";
        }
        public static string CommentsPagePath(string subverse, int submissionID, int? commentID = null, PathOptions options = null)
        {
            //these are all default relative I believe
            options = options == null ? new PathOptions(false, false) : options;

            var commentPath = commentID.HasValue ? $"/{commentID.Value.ToString()}" : "";
            //long
            //return $"/v/{subverse}/comments/{submissionID.ToString()}{commentPath}";
            //short
            return BuildUrlPath(null, options, "v", subverse, submissionID.ToString(), commentPath);
            //return $"/v/{subverse}/{submissionID.ToString()}{commentPath}";
        }
        public static string CommentsPagePath(string subverse, int submissionID, CommentSortAlgorithm sort, object queryString = null, PathOptions options = null)
        {
            //these are all default relative I believe
            options = options == null ? new PathOptions(false, false) : options;

            return BuildUrlPath(null, options, "v", subverse, submissionID.ToString(), sort.ToString().ToLower());
            //return $"/v/{subverse}/{submissionID.ToString()}/{sort.ToString().ToLower()}";
        }

        public static string BasePath(DomainReference domainReference)
        {
            string basePath = "";
            switch (domainReference.Type)
            {
                case DomainType.Set:
                    basePath = $"/s/{domainReference.Name}" + (!String.IsNullOrEmpty(domainReference.OwnerName) ? CONSTANTS.SET_SEPERATOR + $"{domainReference.OwnerName}" : "");
                    break;
                case DomainType.Subverse:
                    basePath = $"/v/{domainReference.Name}";
                    break;
                case DomainType.User:
                    basePath = $"/user/{domainReference.Name}";
                    break;
            }
            return basePath;
        }

        public static string BuildUrlPath(HttpContext context, PathOptions options, params string[] pathParts)
        {
            return BuildUrlPath(context, options, pathParts.AsEnumerable());
        }

        public static string BuildUrlPath(HttpContext context, PathOptions options, IEnumerable<string> pathParts)
        {
            List<string> parts = new List<string>();
            options = PathOptions.EnsureValid(options);
            string leadingPath = "/";

            if (options.FullyQualified)
            {
                var settings = VoatSettings.Instance;
                var domain = settings.SiteDomain;
                string protocol = settings.ForceHTTPS ? "https" : "http"; //default protocol 

                try
                {
                    if (options.ProvideProtocol && context != null && context.Request != null)
                    {
                        //domain = System.Web.HttpContext.Current.Request.Url.Authority;
                        protocol = context.Request.Scheme;
                    }
                }
                catch { }

                if (!String.IsNullOrEmpty(options.ForceDomain))
                {
                    domain = options.ForceDomain;
                }

                var root = String.Format("{0}//{1}", (options.ProvideProtocol ? protocol + ":" : ""), domain);

                parts.Insert(0, root);
                leadingPath = "";
            }

            if (pathParts != null && pathParts.Count() > 0)
            {
                parts.AddRange(pathParts.ToPathParts(null));
            }

            var result = leadingPath + String.Join('/', parts);

            switch (options.Normalization)
            {
                case Normalization.Lower:
                    result = result.ToLower();
                    break;
                case Normalization.Upper:
                    result = result.ToUpper();
                    break;
            }

            if (options.EscapeUrl)
            {
                result = Uri.EscapeUriString(result);
            }

            return result;
        }
        public static string UserProfile(string userName, PathOptions options = null)
        {
            //these are all default relative I believe
            options = options == null ? new PathOptions(false, false) : options;

            if (!String.IsNullOrEmpty(userName))
            {
                return BuildUrlPath(null, options, "user", userName);
            }
            return "#";
        }

        public static string VotePagePath(string subverse, int id, PathOptions options = null)
        {
            //these are all default relative I believe
            options = options == null ? new PathOptions(false, false) : options;

            //long
            //return $"/v/{subverse}/comments/{submissionID.ToString()}{commentPath}";
            //short
            return BuildUrlPath(null, options, "v", subverse, "vote", id.ToString());
            //return $"/v/{subverse}/{submissionID.ToString()}{commentPath}";
        }
    }
}
