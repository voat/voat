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
using System.Net;
using System.Net.Mail;
using System.Web.Http;
using Voat.Caching;
using Voat.Data.Models;
using Voat.Domain;
using Voat.Domain.Query;
using Voat.Models;
using Voat.Models.ApiModels;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class WebApiController : ApiController
    {
        //private readonly voatEntities _db = new voatEntities();

        public WebApiController()
        {
            //Turn off all automatic behavior as we are caching
            //_db.Configuration.ProxyCreationEnabled = false;
            //_db.Configuration.LazyLoadingEnabled = false;
        }

        // GET api/defaultsubverses
        /// <summary>
        ///  This API returns a list of default subverses shown to guests.
        /// </summary>
        [HttpGet]
        public IEnumerable<string> DefaultSubverses()
        {

            IEnumerable<string> defaultSubs = CacheHandler.Instance.Register<IEnumerable<string>>("Legacy:Api:DefaultSubverses",
               new Func<IList<string>>(() =>
               {
                   using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                   {
                       var listOfDefaultSubverses = db.DefaultSubverses.OrderBy(s => s.Order).ToList();
                       return listOfDefaultSubverses.Select(item => item.Subverse).ToList();
                   }
               }), TimeSpan.FromHours(12));
            return defaultSubs;

        }

        // GET api/bannedhostnames
        /// <summary>
        ///  This API returns a list of banned hostnames for link type submissions.
        /// </summary>
        [HttpGet]
        public IEnumerable<string> BannedHostnames()
        {
            IEnumerable<string> bannedSubs = CacheHandler.Instance.Register<IEnumerable<string>>("Legacy:Api:BannedHostnames",
              new Func<IList<string>>(() =>
              {
                  using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                  {
                      var bannedHostnames = db.BannedDomains.OrderBy(s => s.CreationDate).ToList();
                      return bannedHostnames.Select(item => "Hostname: " + item.Domain + ", reason: " + item.Reason + ", added on: " + item.CreationDate + ", added by: " + item.CreatedBy).ToList();
                  }
              }), TimeSpan.FromHours(12));
            return bannedSubs;
        }

        [HttpGet]
        public IEnumerable<string> BannedUsers()
        {
            IEnumerable<string> bannedUsers = CacheHandler.Instance.Register<IEnumerable<string>>("Legacy:Api:BannedUsers",
              new Func<IList<string>>(() =>
              {
                  using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                  {
                      var bannedUserAccounts = db.BannedUsers.OrderBy(s => s.CreationDate).ToList();
                      return bannedUserAccounts.Select(item => "Username: " + item.UserName + ", reason: " + item.Reason + ", added on: " + item.CreationDate + ", added by: " + item.CreatedBy).ToList();
                  }
              }), TimeSpan.FromHours(12));
            return bannedUsers;
        }

        // GET api/ishostnamegloballybanned
        /// <summary>
        ///  This API checks if a hostname is globally banned for link type submissions.
        /// </summary>
        [HttpGet]
        public bool IsHostnameGloballyBanned(string hostnameToCheck)
        {
            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
            {
                var hostname = db.BannedDomains.FirstOrDefault(s => s.Domain == hostnameToCheck);
                return hostname != null;
            }
        }

        // GET api/top200subverses
        /// <summary>
        ///  This API returns top 200 subverses ordered by subscriber count.
        /// </summary>
        [HttpGet]
        public IEnumerable<string> Top200Subverses()
        {
            IEnumerable<string> top200 = CacheHandler.Instance.Register<IEnumerable<string>>("Legacy:Api:Top200Subverses",
                new Func<IList<string>>(() =>
                {
                    using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                    {
                        var top200Subverses = db.Subverses.Where(s => s.IsAdminDisabled != true).OrderByDescending(s => s.SubscriberCount).Take(200);
                        return top200Subverses.Select(item => "Name: " + item.Name + "," + "Description: " + item.Description + "," + "Subscribers: " + item.SubscriberCount + "," + "Created: " + item.CreationDate).ToList();
                    }
                }), TimeSpan.FromMinutes(90), 0);
            return top200;
        }

        // GET api/frontpage
        /// <summary>
        ///  This API returns 100 submissions which are currently shown on Voat frontpage.
        /// </summary>
        [HttpGet]
        public IEnumerable<ApiMessage> Frontpage()
        {

            //IAmAGate: Perf mods for caching
            string cacheKey = String.Format("Legacy:Api:Frontpage").ToLower();
            List<ApiMessage> cacheData = CacheHandler.Instance.Retrieve<List<ApiMessage>>(cacheKey);
            if (cacheData == null)
            {
                cacheData = CacheHandler.Instance.Register(cacheKey, new Func<List<ApiMessage>>(() =>
                {
                    using (var db = new voatEntities())
                    {
                        // get only submissions from default subverses, order by rank
                        var frontpageSubmissions = (from message in db.Submissions
                                                    where !message.IsArchived && !message.IsDeleted && message.Subverse1.IsAdminDisabled != true
                                                    join defaultsubverse in db.DefaultSubverses on message.Subverse equals defaultsubverse.Subverse
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
                                Date = item.CreationDate,
                                Dislikes = (int)item.DownCount,
                                Id = item.ID,
                                LastEditDate = item.LastEditDate,
                                Likes = (int)item.UpCount,
                                Linkdescription = item.Title,
                                MessageContent = (item.Type == 2 ? item.Url : item.Content)
                            };

                            if (item.IsAnonymized || item.Subverse1.IsAnonymized)
                            {
                                resultModel.Name = item.ID.ToString();
                            }
                            else
                            {
                                resultModel.Name = item.UserName;
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
                }), TimeSpan.FromMinutes(5), 4);
            }
            return (IEnumerable<ApiMessage>)cacheData;
        }
        // GET api/subversefrontpage
        /// <summary>
        ///  This API returns 100 submissions which are currently shown on frontpage of a given subverse.
        /// </summary>
        /// <param name="subverse">The name of the subverse for which to fetch submissions.</param>
        [HttpGet]
        public IEnumerable<ApiMessage> SubverseFrontpage(string subverse)
        {
            if (String.IsNullOrEmpty(subverse))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            //IAmAGate: Perf mods for caching
            string cacheKey = String.Format("Legacy:Api:SubverseFrontpage.{0}", subverse).ToLower();
            object cacheData = CacheHandler.Instance.Retrieve<object>(cacheKey);

            if (cacheData == null)
            {

                cacheData = CacheHandler.Instance.Register(cacheKey, new Func<object>(() =>
                {
                    using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                    {
                        // get only submissions from given subverses, order by rank - ignoring messages in any given banned subverse
                        var frontpageSubmissions = (from message in db.Submissions
                                                    where !message.IsDeleted && message.Subverse == subverse && message.Subverse1.IsAdminDisabled != true
                                                    select message)
                                                    .OrderByDescending(s => s.Rank)
                                                    .Take(100)
                                                    .ToList();

                        if (frontpageSubmissions == null || frontpageSubmissions.Count == 0)
                        {
                            return null; // throw new HttpResponseException(HttpStatusCode.NotFound);
                        }

                        var resultList = new List<ApiMessage>();

                        foreach (var item in frontpageSubmissions)
                        {
                            var resultModel = new ApiMessage
                            {
                                CommentCount = item.Comments.Count,
                                Date = item.CreationDate,
                                Dislikes = (int)item.DownCount,
                                Id = item.ID,
                                LastEditDate = item.LastEditDate,
                                Likes = (int)item.UpCount,
                                Linkdescription = item.Title,
                                MessageContent = (item.Type == 2 ? item.Url : item.Content)
                            };

                            if (item.IsAnonymized || item.Subverse1.IsAnonymized)
                            {
                                resultModel.Name = item.ID.ToString();
                            }
                            else
                            {
                                resultModel.Name = item.UserName;
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
                }), TimeSpan.FromMinutes(5), 2);
            }
            //means we didn't find one.
            if (cacheData == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return (IEnumerable<ApiMessage>)cacheData;
        }

        // GET api/singlesubmission
        /// <summary>
        ///  This API returns a single submission for a given submission ID.
        /// </summary>
        /// <param name="id">The ID of submission to fetch.</param>
        [HttpGet]
        public ApiMessage SingleSubmission(int id)
        {

            ApiMessage singleSubmission = CacheHandler.Instance.Register<ApiMessage>(String.Format("Legacy:Api:SingleSubmission.{0}", id),
              new Func<ApiMessage>(() =>
              {
                  using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                  {
                      var submission = db.Submissions.Find(id);

                      if (submission == null || submission.Subverse1.IsAdminDisabled == true)
                      {
                          return null; // throw new HttpResponseException(HttpStatusCode.NotFound);
                      }

                      var resultModel = new ApiMessage
                      {
                          CommentCount = submission.Comments.Count,
                          Id = submission.ID,
                          Date = submission.CreationDate,
                          LastEditDate = submission.LastEditDate,
                          Likes = (int)submission.UpCount,
                          Dislikes = (int)submission.DownCount
                      };

                      if (submission.IsAnonymized || submission.Subverse1.IsAnonymized)
                      {
                          resultModel.Name = submission.ID.ToString();
                      }
                      else
                      {
                          resultModel.Name = submission.UserName;
                      }
                      resultModel.Rank = submission.Rank;
                      resultModel.Thumbnail = submission.Thumbnail;
                      resultModel.Subverse = submission.Subverse;
                      resultModel.Type = submission.Type;
                      resultModel.Title = submission.Title;
                      resultModel.Linkdescription = null;
                      resultModel.MessageContent = (submission.Type == 2 ? submission.Content : submission.Content);

                      return resultModel;
                  }
              }), TimeSpan.FromMinutes(5), 2);

            if (singleSubmission == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return singleSubmission;

        }

        // GET api/singlecomment
        /// <summary>
        ///  This API returns a single comment for a given comment ID.
        /// </summary>
        /// <param name="id">The ID of comment to fetch.</param>
        [HttpGet]
        public ApiComment SingleComment(int id)
        {
            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
            {
                var comment = db.Comments.Find(id);

                if (comment == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                var resultModel = new ApiComment
                {
                    Id = comment.ID,
                    Date = comment.CreationDate,
                    LastEditDate = comment.LastEditDate,
                    Likes = (int)comment.UpCount,
                    Dislikes = (int)comment.DownCount,
                    CommentContent = comment.Content,
                    ParentId = comment.ParentID,
                    MessageId = comment.SubmissionID
                };

                if (comment.Submission.IsAnonymized || comment.Submission.Subverse1.IsAnonymized)
                {
                    resultModel.Name = comment.ID.ToString();
                }
                else
                {
                    resultModel.Name = comment.UserName;
                }

                return resultModel;
            }
        }

        // GET api/sidebarforsubverse
        /// <summary>
        ///  This API returns the sidebar for a subverse.
        /// </summary>
        /// <param name="subverseName">The name of the subverse for which to fetch the sidebar.</param>
        [HttpGet]
        public ApiSubverseInfo SubverseInfo(string subverseName)
        {
            var subverse = DataCache.Subverse.Retrieve(subverseName);

            if (subverse == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // get subscriber count for selected subverse
            var subscriberCount = subverse.SubscriberCount ?? 0;

            var resultModel = new ApiSubverseInfo
            {
                Name = subverse.Name,
                CreationDate = subverse.CreationDate,
                Description = subverse.Description,
                RatedAdult = subverse.IsAdult,
                Sidebar = subverse.SideBar,
                SubscriberCount = subscriberCount,
                Title = subverse.Title,
                Type = subverse.Type
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

            if (String.IsNullOrWhiteSpace(userName) || userName == "deleted" || (userName != "deleted" && !UserHelper.UserExists(userName)))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            ApiUserInfo userInfo = CacheHandler.Instance.Register<ApiUserInfo>(String.Format("Legacy:Api:UserInfo.{0}", userName),
              new Func<ApiUserInfo>(() =>
              {
                  using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                  {
                      var resultModel = new ApiUserInfo();

                      var userData = new Domain.UserData(userName);
                      var info = userData.Information;

                      resultModel.Name = userName;
                      resultModel.CCP = info.CommentPoints.Sum;
                      resultModel.LCP = info.SubmissionPoints.Sum;
                      resultModel.RegistrationDate = info.RegistrationDate;
                      resultModel.Badges = info.Badges.Select(badge => new ApiUserBadge { Awarded = badge.CreationDate, BadgeName = badge.Name }).ToList();

                      return resultModel;
                  }
              }), TimeSpan.FromMinutes(90));
            return userInfo;

        }

        // GET api/badgeinfo
        /// <summary>
        ///  This API returns information about a badge.
        /// </summary>
        /// <param name="badgeId">The badge Id for which to fetch information.</param>
        [HttpGet]
        public ApiBadge BadgeInfo(string badgeId)
        {

            ApiBadge badgeInfo = CacheHandler.Instance.Register<ApiBadge>(String.Format("Legacy:Api:ApiBadge.{0}", badgeId),
             new Func<ApiBadge>(() =>
             {
                 using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                 {
                     var badge = db.Badges.Find(badgeId);

                     if (badge == null)
                     {
                         throw new HttpResponseException(HttpStatusCode.NotFound);
                     }

                     var resultModel = new ApiBadge
                     {
                         BadgeId = badge.ID,
                         BadgeGraphics = badge.Graphic,
                         Name = badge.Name,
                         Title = badge.Title
                     };

                     return resultModel;
                 }
             }), TimeSpan.FromHours(5));
            return badgeInfo;
        }

        // GET api/submissioncomments
        /// <summary>
        ///  This API returns comments for a given submission id.
        /// </summary>
        /// <param name="submissionId">The submission Id for which to fetch comments.</param>
        [HttpGet]
        public IEnumerable<ApiComment> SubmissionComments(int submissionId)
        {
            var submission = DataCache.Submission.Retrieve(submissionId);
            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

            if (submission == null || subverse.IsAdminDisabled == true)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            string cacheKey = String.Format("Legacy:Api:SubmissionComments.{0}", submissionId).ToLower();
            IEnumerable<ApiComment> cacheData = CacheHandler.Instance.Retrieve<IEnumerable<ApiComment>>(cacheKey);

            if (cacheData == null)
            {
                cacheData = CacheHandler.Instance.Register(cacheKey, new Func<IEnumerable<ApiComment>>(() =>
                {

                    using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                    {
                        var firstComments = from f in db.Comments
                                            let commentScore = (int)f.UpCount - (int)f.DownCount
                                            where f.SubmissionID == submissionId && f.ParentID == null
                                            orderby commentScore descending
                                            select f;

                        var resultList = new List<ApiComment>();

                        foreach (var firstComment in firstComments.Take(10))
                        {
                            //do not show deleted comments unless they have replies
                            if (firstComment.IsDeleted && db.Comments.Count(a => a.ID == submissionId && a.ParentID == firstComment.ID) == 0)
                            {
                                continue;
                            }

                            var resultModel = new ApiComment
                            {
                                Id = firstComment.ID,
                                Date = firstComment.CreationDate,
                                Dislikes = (int)firstComment.DownCount,
                                LastEditDate = firstComment.LastEditDate,
                                Likes = (int)firstComment.UpCount,
                                MessageId = firstComment.SubmissionID,
                                ParentId = firstComment.ParentID,
                                CommentContent = firstComment.Content
                            };

                            if (firstComment.Submission.IsAnonymized || firstComment.Submission.Subverse1.IsAnonymized)
                            {
                                resultModel.Name = firstComment.ID.ToString();
                            }
                            else
                            {
                                resultModel.Name = firstComment.UserName;
                            }

                            // TODO
                            // fetch child comments

                            resultList.Add(resultModel);
                        }

                        return resultList;

                    }
                }), TimeSpan.FromMinutes(5));

            }
            return cacheData;
        }

        // GET api/top100imagesByDate
        [HttpGet]
        public ImageBucket Top100ImagesByDate()
        {
            IEnumerable<ResponseItem> top100ImagesByDate = CacheHandler.Instance.Register<IEnumerable<ResponseItem>>("Legacy:Api:Top100ImagesByDate",
                () =>
                {
                    using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                    {
                        var query = from pro in db.Submissions.Where(s => s.Type == 2 && (s.Url.EndsWith(".png") || s.Url.EndsWith(".jpg")) && s.UpCount > 10 && !s.Subverse1.IsAdult).OrderByDescending(s => s.CreationDate).Take(100)
                                    select new ResponseItem
                                    {
                                        SubmissionId = pro.ID,
                                        Alt = pro.Title, 
                                        Img = pro.Url,
                                        DownVotes = pro.DownCount,
                                        SubmittedBy = pro.UserName,
                                        SubmittedOn = pro.CreationDate,
                                        Subverse = pro.Subverse,
                                        UpVotes = pro.UpCount
                                    };
                        return query.ToList();
                    }
                }, TimeSpan.FromMinutes(30), 0);

            var bucket = new ImageBucket();
            var itemList = top100ImagesByDate.ToList();
            bucket.Items = itemList;
            return bucket;
        }

        public class ResponseItem
        {
            public int SubmissionId { get; set; }
            public string Img { get; set; }
            public string Alt { get; set; }
            public string Subverse { get; set; }
            public string SubmittedBy { get; set; }
            public DateTime SubmittedOn { get; set; }
            public long UpVotes { get; set; }
            public long DownVotes { get; set; }
        }

        public class ImageBucket
        {
            public List<ResponseItem> Items { get; set; }
        }
    }
}