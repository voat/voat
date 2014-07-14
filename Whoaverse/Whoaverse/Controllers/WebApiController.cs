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
using System.Net;
using System.Web.Http;
using Whoaverse.Models;
using Whoaverse.Models.ApiModels;

namespace Whoaverse.Controllers
{
    public class WebApiController : ApiController
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET api/defaultsubverses
        /// <summary>
        ///  This API returns a list of default subverses shown to guests.
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<string> DefaultSubverses()
        {
            var listOfDefaultSubverses = db.Defaultsubverses.OrderBy(s => s.position).ToList();

            List<string> tmpList = new List<string>();
            foreach (var item in listOfDefaultSubverses)
            {
                tmpList.Add(item.name);
            }

            return tmpList;
        }

        // GET api/bannedhostnames
        /// <summary>
        ///  This API returns a list of banned hostnames for link type submissions.
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<string> BannedHostnames()
        {
            var bannedHostnames = db.Banneddomains.OrderBy(s => s.Added_on).ToList();

            List<string> tmpList = new List<string>();
            foreach (var item in bannedHostnames)
            {
                tmpList.Add("Hostname: " + item.Hostname + ", reason: " + item.Reason + ", added on: " + item.Added_on + ", added by: " + item.Added_by);
            }

            return tmpList;
        }

        // GET api/top200subverses
        /// <summary>
        ///  This API returns top 200 subverses ordered by subscriber count.
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<string> Top200Subverses()
        {
            var top200Subverses = db.Subverses.OrderByDescending(s => s.subscribers).ToList();

            List<string> resultList = new List<string>();
            foreach (var item in top200Subverses)
            {
                resultList.Add(
                    "Name: " + item.name + "," +
                    "Description: " + item.description + "," +
                    "Subscribers: " + item.subscribers + "," +
                    "Created: " + item.creation_date
                    );
            }

            return resultList;
        }

        // GET api/frontpage
        /// <summary>
        ///  This API returns 100 submissions which are currently shown on WhoaVerse frontpage.
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<ApiMessage> Frontpage()
        {
            //get only submissions from default subverses, order by rank
            var frontpageSubmissions = (from message in db.Messages
                                        join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                        where message.Name != "deleted"
                                        select message)
                               .Distinct()
                               .OrderByDescending(s => s.Rank).Take(100).ToList();

            List<ApiMessage> resultList = new List<ApiMessage>();

            foreach (var item in frontpageSubmissions)
            {
                ApiMessage resultModel = new ApiMessage();

                resultModel.CommentCount = item.Comments.Count;
                resultModel.Date = item.Date;
                resultModel.Dislikes = item.Dislikes;
                resultModel.Id = item.Id;
                resultModel.LastEditDate = item.LastEditDate;
                resultModel.Likes = item.Likes;
                resultModel.Linkdescription = item.Linkdescription;
                resultModel.MessageContent = item.MessageContent;
                resultModel.Name = item.Name;
                resultModel.Rank = item.Rank;
                resultModel.Subverse = item.Subverse;
                resultModel.Thumbnail = item.Thumbnail;
                resultModel.Title = item.Title;
                resultModel.Type = item.Type;

                resultList.Add(resultModel);
            }

            return resultList;
        }

        // GET api/subversefrontpage
        /// <summary>
        ///  This API returns 100 submissions which are currently shown on frontpage of a given subverse.
        /// </summary>
        /// <param name="subverse">The name of the subverse for which to fetch submissions.</param>
        [System.Web.Http.HttpGet]
        public IEnumerable<ApiMessage> SubverseFrontpage(string subverse)
        {
            //get only submissions from given subverses, order by rank
            var frontpageSubmissions = (from message in db.Messages
                                        where message.Name != "deleted" && message.Subverse == subverse
                                        select message)
                               .Distinct()
                               .OrderByDescending(s => s.Rank).Take(100).ToList();

            if (frontpageSubmissions == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            List<ApiMessage> resultList = new List<ApiMessage>();

            foreach (var item in frontpageSubmissions)
            {
                ApiMessage resultModel = new ApiMessage();

                resultModel.CommentCount = item.Comments.Count;
                resultModel.Date = item.Date;
                resultModel.Dislikes = item.Dislikes;
                resultModel.Id = item.Id;
                resultModel.LastEditDate = item.LastEditDate;
                resultModel.Likes = item.Likes;
                resultModel.Linkdescription = item.Linkdescription;
                resultModel.MessageContent = item.MessageContent;
                resultModel.Name = item.Name;
                resultModel.Rank = item.Rank;
                resultModel.Subverse = item.Subverse;
                resultModel.Thumbnail = item.Thumbnail;
                resultModel.Title = item.Title;
                resultModel.Type = item.Type;

                resultList.Add(resultModel);
            }

            return resultList;
        }

        // GET api/singlesubmission
        /// <summary>
        ///  This API returns a single submission for a given submission ID.
        /// </summary>
        /// <param name="id">The ID of submission to fetch.</param>
        [System.Web.Http.HttpGet]
        public ApiMessage SingleSubmission(int id)
        {
            Message submission = db.Messages.Find(id);

            if (submission == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            ApiMessage resultModel = new ApiMessage();

            resultModel.CommentCount = submission.Comments.Count;
            resultModel.Id = submission.Id;
            resultModel.Date = submission.Date;
            resultModel.LastEditDate = submission.LastEditDate;
            resultModel.Likes = submission.Likes;
            resultModel.Dislikes = submission.Dislikes;
            resultModel.Rank = submission.Rank;
            resultModel.Thumbnail = submission.Thumbnail;
            resultModel.Subverse = submission.Subverse;
            resultModel.Type = submission.Type;
            resultModel.Title = submission.Title;
            resultModel.Linkdescription = submission.Linkdescription;
            resultModel.MessageContent = submission.MessageContent;

            return resultModel;
        }

        // GET api/singlecomment
        /// <summary>
        ///  This API returns a single comment for a given comment ID.
        /// </summary>
        /// <param name="id">The ID of comment to fetch.</param>
        [System.Web.Http.HttpGet]
        public ApiComment SingleComment(int id)
        {
            Comment comment = db.Comments.Find(id);

            if (comment == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            ApiComment resultModel = new ApiComment();

            resultModel.Id = comment.Id;
            resultModel.Date = comment.Date;
            resultModel.LastEditDate = comment.LastEditDate;
            resultModel.Likes = comment.Likes;
            resultModel.Dislikes = comment.Dislikes;
            resultModel.CommentContent = comment.CommentContent;
            resultModel.ParentId = comment.ParentId;
            resultModel.MessageId = comment.MessageId;
            resultModel.Name = comment.Name;

            return resultModel;
        }

        // GET api/sidebarforsubverse
        /// <summary>
        ///  This API returns the sidebar for a subverse.
        /// </summary>
        /// <param name="subverseName">The name of the subverse for which to fetch the sidebar.</param>
        [System.Web.Http.HttpGet]
        public ApiSubverseInfo SubverseInfo(string subverseName)
        {
            Subverse subverse = db.Subverses.Find(subverseName);

            if (subverse == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // get subscriber count for selected subverse
            int subscriberCount = db.Subscriptions.AsEnumerable()
                                .Where(r => r.SubverseName.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
                                .Count();

            ApiSubverseInfo resultModel = new ApiSubverseInfo();

            resultModel.Name = subverse.name;
            resultModel.CreationDate = subverse.creation_date;
            resultModel.Description = subverse.description;
            resultModel.RatedAdult = subverse.rated_adult;
            resultModel.Sidebar = subverse.sidebar;
            resultModel.SubscriberCount = subscriberCount;
            resultModel.Title = subverse.title;
            resultModel.Type = subverse.type;

            return resultModel;
        }

        // GET api/userinfo
        /// <summary>
        ///  This API returns basic information about a user.
        /// </summary>
        /// <param name="userName">The username for which to fetch basic information.</param>
        [System.Web.Http.HttpGet]
        public ApiUserInfo UserInfo(string userName)
        {
            if (userName != "deleted" && !Whoaverse.Utils.User.UserExists(userName))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (userName == "deleted")
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            ApiUserInfo resultModel = new ApiUserInfo();

            List<Userbadge> userBadgesList = Utils.User.UserBadges(userName);
            List<ApiUserBadge> resultBadgesList = new List<ApiUserBadge>();

            foreach (var item in userBadgesList)
            {
                ApiUserBadge tmpBadge = new ApiUserBadge();
                tmpBadge.Awarded = item.Awarded;
                tmpBadge.BadgeName = item.Badge.BadgeName;

                resultBadgesList.Add(tmpBadge);
            }

            resultModel.Name = userName;
            resultModel.CCP = Utils.Karma.CommentKarma(userName);
            resultModel.LCP = Utils.Karma.LinkKarma(userName);
            resultModel.RegistrationDate = Utils.User.GetUserRegistrationDateTime(userName);
            resultModel.Badges = resultBadgesList;

            return resultModel;
        }

        // GET api/badgeinfo
        /// <summary>
        ///  This API returns information about a badge.
        /// </summary>
        /// <param name="badgeId">The badge Id for which to fetch information.</param>
        [System.Web.Http.HttpGet]
        public ApiBadge BadgeInfo(string badgeId)
        {
            Badge badge = db.Badges.Find(badgeId);

            if (badge == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            ApiBadge resultModel = new ApiBadge();

            resultModel.BadgeId = badge.BadgeId;
            resultModel.BadgeGraphics = badge.BadgeGraphics;
            resultModel.Name = badge.BadgeName;
            resultModel.Title = badge.BadgeTitle;

            return resultModel;
        }

        // GET api/submissioncomments
        /// <summary>
        ///  This API returns comments for a given submission id.
        /// </summary>
        /// <param name="submissionId">The submission Id for which to fetch comments.</param>
        [System.Web.Http.HttpGet]
        public IEnumerable<ApiComment> SubmissionComments(int submissionId) {
            Message submission = db.Messages.Find(submissionId);

            if (submission == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var firstComments = from f in submission.Comments
                                let commentScore = f.Likes - f.Dislikes
                                where f.ParentId == null
                                orderby commentScore descending
                                select f;

            List<ApiComment> resultList = new List<ApiComment>();

            foreach (var firstComment in firstComments)
            {
                //do not show deleted comments unless they have replies
                if (firstComment.Name == "deleted" && submission.Comments.Where(A => A.ParentId == firstComment.Id).Count() == 0)
                {
                    continue;
                }

                //@Html.Partial("_SubmissionComment", Model, new ViewDataDictionary { { "CommentId", firstComment.Id }, { "CCP", commentContributionPoints }, { "parentIsHidden", true } })
                //int commentId = Convert.ToInt32(ViewData["CommentId"]);
                //var singleComment = from c in submission.Comments where c.Id == commentId select c;
                //var commentModel = singleComment.FirstOrDefault();

                ApiComment resultModel = new ApiComment();

                resultModel.Id = firstComment.Id;
                resultModel.Date = firstComment.Date;
                resultModel.Dislikes = firstComment.Dislikes;
                resultModel.LastEditDate = firstComment.LastEditDate;
                resultModel.Likes = firstComment.Likes;
                resultModel.MessageId = firstComment.MessageId;
                resultModel.ParentId = firstComment.ParentId;
                resultModel.CommentContent = firstComment.CommentContent;
                resultModel.Name = firstComment.Name;

                // TODO
                // fetch child comments

                resultList.Add(resultModel);
            }

            return resultList;
        }
    }
}