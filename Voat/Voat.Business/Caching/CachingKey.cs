using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static string SubverseModerators(string subverse)
        {
            return String.Format("Subverse:Mods:{0}", subverse);
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
            return String.Format("Comment:Count{0}", submissionID);
        }
        public static string CommentTree(int submissionID)
        {
            return String.Format("Comment:Tree:{0}", submissionID);
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
        public static string UserSubmissionVotes(string userName)
        {
            return String.Format("User:Votes:Submissions:{0}", userName);
        }

        public static string UserBlocks(string userName)
        {
            return String.Format("User:Blocks:{0}", userName);
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
        
        public static string ApiStreamLastCallDate(ContentType contentType, string userName)
        {
            return String.Format("Api:Stream:{0}:{1}", String.IsNullOrEmpty(userName) ? "unknown" : userName, contentType.ToString());
        }

        public static string SiteSearch(string subverse, string phrase)
        {
            return String.Format("Search:{0}:{1}", String.IsNullOrEmpty(subverse) ? "_" : subverse, phrase);
        }
        public static string SiteSearch(string phrase)
        {
            return SiteSearch("", phrase);
        }

    }
}
