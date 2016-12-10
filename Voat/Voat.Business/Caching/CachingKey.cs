using System;
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

        public static string UserSet(string setName)
        {
            return String.Format("User:Set:{0}", setName);
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
            return String.Format("User:Profile:Overview:{0}", userName);
        }
        public static string UserSavedItems(ContentType type, string userName)
        {
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

    }
}
