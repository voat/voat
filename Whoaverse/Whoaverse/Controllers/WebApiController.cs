/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Voat.Models;
using Voat.Models.ApiModels;
using Voat.Utils;

namespace Voat.Controllers
{
    public class WebApiController : ApiController
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        // GET api/defaultsubverses
        /// <summary>
        ///  This API returns a list of default subverses shown to guests.
        /// </summary>
        [HttpGet]
        public IEnumerable<string> DefaultSubverses()
        {
            var listOfDefaultSubverses = _db.Defaultsubverses.OrderBy(s => s.position).ToList();

            return listOfDefaultSubverses.Select(item => item.name);
        }

        // GET api/bannedhostnames
        /// <summary>
        ///  This API returns a list of banned hostnames for link type submissions.
        /// </summary>
        [HttpGet]
        public IEnumerable<string> BannedHostnames()
        {
            var bannedHostnames = _db.Banneddomains.OrderBy(s => s.Added_on).ToList();

            return bannedHostnames.Select(item => "Hostname: " + item.Hostname + ", reason: " + item.Reason + ", added on: " + item.Added_on + ", added by: " + item.Added_by);
        }

        // GET api/ishostnamegloballybanned
        /// <summary>
        ///  This API checks if a hostname is globally banned for link type submissions.
        /// </summary>
        [HttpGet]
        public bool IsHostnameGloballyBanned(string hostnameToCheck)
        {
            var hostname = _db.Banneddomains.FirstOrDefault(s => s.Hostname == hostnameToCheck);
            return hostname != null;
        }

        // GET api/top200subverses
        /// <summary>
        ///  This API returns top 200 subverses ordered by subscriber count.
        /// </summary>
        [HttpGet]
        public IEnumerable<string> Top200Subverses()
        {
            var top200Subverses = _db.Subverses.Where(s => s.admin_disabled != true).OrderByDescending(s => s.subscribers).ToList();

            return top200Subverses.Select(item => "Name: " + item.name + "," + "Description: " + item.description + "," + "Subscribers: " + item.subscribers + "," + "Created: " + item.creation_date);
        }

        // GET api/frontpage
        /// <summary>
        ///  This API returns 100 submissions which are currently shown on Voat frontpage.
        /// </summary>
        [HttpGet]
        public IEnumerable<ApiMessage> Frontpage()
        {
            // get only submissions from default subverses, order by rank
            var frontpageSubmissions = (from message in _db.Messages.Include("subverses")
                                        where message.Name != "deleted" && message.Subverses.admin_disabled != true
                                        join defaultsubverse in _db.Defaultsubverses on message.Subverse equals defaultsubverse.name                                        
                                        select message)
                                        .OrderByDescending(s => s.Rank)
                                        .Take(100)
                                        .ToList();

            var resultList = new List<ApiMessage>();

            foreach (var item in frontpageSubmissions)
            {
                var resultModel = new ApiMessage
                {
                    CommentCount = item.Comments.Count,
                    Date = item.Date,
                    Dislikes = item.Dislikes,
                    Id = item.Id,
                    LastEditDate = item.LastEditDate,
                    Likes = item.Likes,
                    Linkdescription = item.Linkdescription,
                    MessageContent = item.MessageContent
                };

                if (item.Anonymized || item.Subverses.anonymized_mode)
                {
                    resultModel.Name = item.Id.ToString();
                }
                else
                {
                    resultModel.Name = item.Name;
                }                
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
        [HttpGet]
        public IEnumerable<ApiMessage> SubverseFrontpage(string subverse)
        {
            // get only submissions from given subverses, order by rank - ignoring messages in any given banned subverse
            var frontpageSubmissions = (from message in _db.Messages.Include("subverses")
                                        where message.Name != "deleted" && message.Subverse == subverse && message.Subverses.admin_disabled != true
                                        select message)
                                        .OrderByDescending(s => s.Rank)
                                        .Take(100)
                                        .ToList();

            if (frontpageSubmissions == null || frontpageSubmissions.Count == 0)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var resultList = new List<ApiMessage>();

            foreach (var item in frontpageSubmissions)
            {
                var resultModel = new ApiMessage
                {
                    CommentCount = item.Comments.Count,
                    Date = item.Date,
                    Dislikes = item.Dislikes,
                    Id = item.Id,
                    LastEditDate = item.LastEditDate,
                    Likes = item.Likes,
                    Linkdescription = item.Linkdescription,
                    MessageContent = item.MessageContent
                };

                if (item.Anonymized || item.Subverses.anonymized_mode)
                {
                    resultModel.Name = item.Id.ToString();
                }
                else
                {
                    resultModel.Name = item.Name;
                }                
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
        [HttpGet]
        public ApiMessage SingleSubmission(int id)
        {
            var submission = _db.Messages.Find(id);

            if (submission == null || submission.Subverses.admin_disabled == true)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var resultModel = new ApiMessage
            {
                CommentCount = submission.Comments.Count,
                Id = submission.Id,
                Date = submission.Date,
                LastEditDate = submission.LastEditDate,
                Likes = submission.Likes,
                Dislikes = submission.Dislikes
            };

            if (submission.Anonymized || submission.Subverses.anonymized_mode)
            {
                resultModel.Name = submission.Id.ToString();
            }
            else
            {
                resultModel.Name = submission.Name;
            }            
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
        [HttpGet]
        public ApiComment SingleComment(int id)
        {
            var comment = _db.Comments.Find(id);

            if (comment == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var resultModel = new ApiComment
            {
                Id = comment.Id,
                Date = comment.Date,
                LastEditDate = comment.LastEditDate,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
                CommentContent = comment.CommentContent,
                ParentId = comment.ParentId,
                MessageId = comment.MessageId
            };

            if (comment.Message.Anonymized || comment.Message.Subverses.anonymized_mode)
            {
                resultModel.Name = comment.Id.ToString();
            }
            else
            {
                resultModel.Name = comment.Name;
            }            

            return resultModel;
        }

        // GET api/sidebarforsubverse
        /// <summary>
        ///  This API returns the sidebar for a subverse.
        /// </summary>
        /// <param name="subverseName">The name of the subverse for which to fetch the sidebar.</param>
        [HttpGet]
        public ApiSubverseInfo SubverseInfo(string subverseName)
        {
            var subverse = _db.Subverses.Find(subverseName);

            if (subverse == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // get subscriber count for selected subverse
            var subscriberCount = _db.Subscriptions.AsEnumerable().Count(r => r.SubverseName.Equals(subverseName, StringComparison.OrdinalIgnoreCase));

            var resultModel = new ApiSubverseInfo
            {
                Name = subverse.name,
                CreationDate = subverse.creation_date,
                Description = subverse.description,
                RatedAdult = subverse.rated_adult,
                Sidebar = subverse.sidebar,
                SubscriberCount = subscriberCount,
                Title = subverse.title,
                Type = subverse.type
            };

            return resultModel;
        }

        // GET api/userinfo
        /// <summary>
        ///  This API returns basic information about a user.
        /// </summary>
        /// <param name="userName">The username for which to fetch basic information.</param>
        [HttpGet]
        public ApiUserInfo UserInfo(string userName)
        {
            if (userName != "deleted" && !Utils.User.UserExists(userName))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (userName == "deleted")
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var resultModel = new ApiUserInfo();

            var userBadgesList = Utils.User.UserBadges(userName);
            var resultBadgesList = userBadgesList.Select(item => new ApiUserBadge {Awarded = item.Awarded, BadgeName = item.Badge.BadgeName}).ToList();

            resultModel.Name = userName;
            resultModel.CCP = Karma.CommentKarma(userName);
            resultModel.LCP = Karma.LinkKarma(userName);
            resultModel.RegistrationDate = Utils.User.GetUserRegistrationDateTime(userName);
            resultModel.Badges = resultBadgesList;

            return resultModel;
        }

        // GET api/badgeinfo
        /// <summary>
        ///  This API returns information about a badge.
        /// </summary>
        /// <param name="badgeId">The badge Id for which to fetch information.</param>
        [HttpGet]
        public ApiBadge BadgeInfo(string badgeId)
        {
            var badge = _db.Badges.Find(badgeId);

            if (badge == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var resultModel = new ApiBadge
            {
                BadgeId = badge.BadgeId,
                BadgeGraphics = badge.BadgeGraphics,
                Name = badge.BadgeName,
                Title = badge.BadgeTitle
            };

            return resultModel;
        }

        // GET api/submissioncomments
        /// <summary>
        ///  This API returns comments for a given submission id.
        /// </summary>
        /// <param name="submissionId">The submission Id for which to fetch comments.</param>
        [HttpGet]
        public IEnumerable<ApiComment> SubmissionComments(int submissionId) {
            var submission = _db.Messages.Find(submissionId);

            if (submission == null || submission.Subverses.admin_disabled == true)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var firstComments = from f in submission.Comments
                                let commentScore = f.Likes - f.Dislikes
                                where f.ParentId == null
                                orderby commentScore descending
                                select f;

            var resultList = new List<ApiComment>();

            foreach (var firstComment in firstComments.Take(10))
            {
                //do not show deleted comments unless they have replies
                if (firstComment.Name == "deleted" && submission.Comments.Count(a => a.ParentId == firstComment.Id) == 0)
                {
                    continue;
                }

                var resultModel = new ApiComment
                {
                    Id = firstComment.Id,
                    Date = firstComment.Date,
                    Dislikes = firstComment.Dislikes,
                    LastEditDate = firstComment.LastEditDate,
                    Likes = firstComment.Likes,
                    MessageId = firstComment.MessageId,
                    ParentId = firstComment.ParentId,
                    CommentContent = firstComment.CommentContent
                };

                if (firstComment.Message.Anonymized || firstComment.Message.Subverses.anonymized_mode)
                {
                    resultModel.Name = firstComment.Id.ToString();
                }
                else
                {
                    resultModel.Name = firstComment.Name;
                }                

                // TODO
                // fetch child comments

                resultList.Add(resultModel);
            }

            return resultList;
        }
    }
}