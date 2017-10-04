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
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Caching
{
    public static class CachingKey
    {
        public static string Subverse(string subverse)
        {
            return String.Format("Subverse:Object:{0}", subverse);
        }

        public static string SubverseInformation(string subverse)
        {
            return String.Format("Subverse:Info:{0}", subverse);
        }

        public static string SubverseStylesheet(string subverse)
        {
            return String.Format("Subverse:Stylesheet:{0}", subverse);
        }
        public static string SubverseFlair(string subverse)
        {
            return String.Format("Subverse:Flair:{0}", subverse);
        }
        public static string SubverseRuleSet(string subverse)
        {
            return String.Format("Subverse:RuleSet:{0}", subverse);
        }
        public static string SubverseModerators(string subverse)
        {
            return String.Format("Subverse:Mods:{0}", subverse);
        }

        public static string SubverseUserBans(string subverse)
        {
            return String.Format("Subverse:Bans:{0}", subverse);
        }
        public static string SubverseHighestRank(string subverse)
        {
            return String.Format("Subverse:HighestRank:{0}", subverse).ToLower();
        }
        public static string Submission(int submissionID)
        {
            return String.Format("Submission:{0}", submissionID);
        }
        public static string ActiveSessionCount(DomainReference domainReference)
        {
            return String.Format("ActiveSessionCount:{0}:{1}", domainReference.Type, domainReference.Name);
        }
        public static string Comment(int commentID)
        {
            return String.Format("Comment:{0}", commentID);
        }

        public static string CommentCount(int submissionID)
        {
            return String.Format("Comment:Count:{0}", submissionID);
        }

        public static string CommentTree(int submissionID)
        {
            return String.Format("Comment:Tree:{0}", submissionID);
        }

        public static string Set(string setName, string setOwner)
        {
            return String.Format("Set:{0}:{1}", (String.IsNullOrEmpty(setOwner) ? "_" : setOwner), setName);
        }

        public static string UserPreferences(string userName)
        {
            return String.Format("User:Preferences:{0}", userName);
        }

        public static string UserSubscriptions(string userName)
        {
            return String.Format("User:Subscriptions:{0}", userName);
        }

        public static string UserInformation(string userName)
        {
            return String.Format("User:Information:{0}", userName);
        }

        public static string UserData(string userName)
        {
            return String.Format("User:Data:{0}", userName);
        }

        public static string UserCommentVotes(string userName, int submissionID)
        {
            return String.Format("User:Votes:{0}:{1}", submissionID, userName);
        }

        public static string UserSavedComments(string userName, int submissionID)
        {
            return String.Format("User:SavedComments:{0}:{1}", submissionID, userName);
        }

        public static string UserContent(string userName, ContentType type, SearchOptions options)
        {
            return String.Format("User:Profile:{0}:{1}:{2}", userName, type.ToString(), options.ToString());
        }
        public static string UserOverview(string userName)
        {
            return String.Format("User:Overview:{0}", userName);
        }
        public static string UserSavedItems(ContentType type, string userName)
        {
            userName.EnsureNotNullOrEmpty(nameof(userName));
            return String.Format("User:Saved:{0}:{1}", type, userName);
        }
        public static string UserSubmissionVotes(string userName)
        {
            return String.Format("User:Votes:Submissions:{0}", userName);
        }
        public static string UserContributionPointsForSubverse(string userName, ContentType type, string subverse)
        {
            return String.Format("User:ContributionPoints:{0}:{1}:{2}", type.ToString(), userName, subverse);
        }

        public static string UserBlocks(string userName)
        {
            return String.Format("User:Blocks:{0}", userName);
        }

        public static string UserRecord(string userName)
        {
            return String.Format("User:Record:{0}", userName);
        }

        public static string ApiPermissionPolicy(int? apiPermissionPolicyID)
        {
            return String.Format("Api:Policy:{0}", apiPermissionPolicyID.HasValue ? apiPermissionPolicyID.Value.ToString() : "default");
        }

        public static string ApiThrottlePolicy(int apiThrottlePolicyID)
        {
            return String.Format("Api:Throttle:{0}", apiThrottlePolicyID.ToString());
        }

        public static string ApiClient(string apiKey)
        {
            return String.Format("Api:Client:{0}", apiKey);
        }

        public static string ApiCorsPolicies()
        {
            return "Api:Cors";
        }

        public static string ApiStreamLastCallDate(ContentType contentType, string userName, string subverse)
        {
            return String.Format("Api:Stream:{0}:{1}:{2}", String.IsNullOrEmpty(userName) ? "unknown" : userName, contentType.ToString(), (String.IsNullOrEmpty(subverse) ? "All" : subverse));
        }

        public static string SiteSearch(string subverse, string phrase)
        {
            return String.Format("Search:{0}:{1}", String.IsNullOrEmpty(subverse) ? "_" : subverse, phrase);
        }

        public static string SiteSearch(string phrase)
        {
            return SiteSearch("", phrase);
        }

        public static string AdCache()
        {
            return String.Format("System:AdCache");
        }
        public static string Filters()
        {
            return String.Format("System:FilterCache");
        }

        

        public static string RssFeed(string subverse)
        {
            return String.Format("Subverse:Rss:{0}", String.IsNullOrWhiteSpace(subverse) ? "all" : subverse);
        }

        public static string StickySubmission(string subverse)
        {
            return String.Format("Subverse:Sticky:{0}", String.IsNullOrWhiteSpace(subverse) ? "all" : subverse);
        }

        public static string ReportKey(ContentType type, int ID)
        {
            return $"Reports:{type.ToString()}:{ID.ToString()}";
        }

        public static string ReportCountUserKey(ContentType type, string userName)
        {
            return $"Reports:{type.ToString()}:{userName}";
        }
        public static string DomainSearch(string domain, int page, string sort)
        {
            return $"Domain:{domain}:{sort}:page{page}";
        }
        public static string DomainObjectSearch(DomainType domainType, SearchOptions options)
        {
            return $"DomainSearch:{domainType.ToString()}:page{options.Page}:{options.Sort.ToString()}:{options.Phrase}";
        }
        public static string ModLogBannedUsers(string subverse, SearchOptions options)
        {
            return $"Subverse:ModLog:{subverse}:Banned{options.ToString(":{0}", "page=0")}";
        }
        public static string ModLogSubmissions(string subverse, SearchOptions options)
        {
            return $"Subverse:ModLog:{subverse}:Submissions{options.ToString(":{0}", "page=0")}";
        }
        public static string ModLogComments(string subverse, SearchOptions options)
        {
            return $"Subverse:ModLog:{subverse}:Comments{options.ToString(":{0}", "page=0")}";
        }
        public static string Vote(int id)
        {
            return $"Vote:Object:{id}";
        }
        public static string Votes(string subverse, SearchOptions options)
        {
            return $"Vote:List:{subverse}:page{options.Page}:{options.Sort.ToString()}:{options.Phrase}";
        }
        public static string VoteStatistics(int id)
        {
            return $"Vote:Stats:{id}";
        }
        public static string BannedDomains()
        {
            return "Banned:Domains";
        }
        public static string BannedUsers()
        {
            return "Banned:Users";
        }
        public class Statistics
        {
            public static string UserVotesGiven(SearchOptions options)
            {
                return $"Statistics:UserVotesGiven";
            }
            public static string UserVotesReceived(SearchOptions options)
            {
                return $"Statistics:UserVotesReceived";
            }
            public static string HighestVotedContent(SearchOptions options)
            {
                return $"Statistics:HighestVotedContent";
            }
        }
    }
}
