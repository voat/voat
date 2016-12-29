using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Models;
using Voat.UI.Utilities;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class RssController : BaseController
    {

        // GET: rss/{subverseName}
        public async Task<ActionResult> Rss(string subverseName)
        {

            //var submissions = CacheHandler.Instance.Register<List<Submission>>(CachingKey.RssFeed(subverseName), new Func<List<Submission>>(() => {
            //    var result = new List<Submission>();
            //    using (var _db = new voatEntities())
            //    {
            //        _db.EnableCacheableOutput();

            //        if (subverseName != null && subverseName != "all")
            //        {
            //            // return only frontpage submissions from a given subverse
            //            var subverse = DataCache.Subverse.Retrieve(subverseName); // _db.Subverse.Find(subverseName);
            //            if (subverse != null)
            //            {

            //                //HACK: Disable subverse
            //                if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            //                {
            //                    //ViewBag.Subverse = subverse.Name;
            //                    //return SubverseDisabledErrorView();
            //                    return result;
            //                }

            //                result = (from message in _db.Submissions
            //                               where !message.IsDeleted && message.Subverse == subverse.Name
            //                               select message)
            //                               .OrderByDescending(s => s.Rank)
            //                               .Take(25)
            //                               .ToList();
            //            }
            //        }
            //        else if (subverseName == "all")
            //        {
            //            // return submissions from all subs
            //            result = (from message in _db.Submissions
            //                           join subverse in _db.Subverses on message.Subverse equals subverse.Name
            //                           where !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && message.Rank > 0.00009
            //                           where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
            //                           select message).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).Take(25).ToList();
            //        }
            //        else
            //        {
            //            // return site-wide frontpage submissions
            //            result = (from message in _db.Submissions
            //                           where !message.IsDeleted
            //                           join defaultsubverse in _db.DefaultSubverses on message.Subverse equals defaultsubverse.Subverse
            //                           select message)
            //                           .OrderByDescending(s => s.Rank)
            //                           .Take(25)
            //                           .ToList();
            //        }
            //    }
            //    return result;
            //}), TimeSpan.FromMinutes(30));

            subverseName = String.IsNullOrEmpty(subverseName) ? AGGREGATE_SUBVERSE.ALL : subverseName;

            var q = new QuerySubmissions(subverseName, new Data.SearchOptions());
            var submissions = await q.ExecuteAsync();

            var feed = new SyndicationFeed("Voat", "Have your say", new Uri("http://www.voat.co"))
            {
                Language = "en-US",
                ImageUrl =
                    new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority +
                            "/Graphics/voat-logo.png")
            };

            var feedItems = new List<SyndicationItem>();

            foreach (var submission in submissions)
            {
                var commentsUrl = new Uri("https://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse + "/comments/" + submission.ID);
                var subverseUrl = new Uri("https://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse);
                var authorUrl = new Uri("https://" + System.Web.HttpContext.Current.Request.Url.Authority + "/user/" + submission.UserName);

                var authorName = submission.UserName;
                // submission type submission
                if (submission.IsAnonymized)
                {
                    authorName = submission.ID.ToString(CultureInfo.InvariantCulture);
                    authorUrl = new Uri("https://" + System.Web.HttpContext.Current.Request.Url.Authority);
                }

                if (submission.Type == Voat.Domain.Models.SubmissionType.Text)
                {
                   
                    var item = new SyndicationItem(
                    submission.Title,
                    submission.Content + "</br>" + "Submitted by " + "<a href='" + authorUrl + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.CommentCount + " comments",
                    commentsUrl,
                    submission.ID.ToString(CultureInfo.InvariantCulture),
                    submission.CreationDate);
                    feedItems.Add(item);
                }
                else
                {
                    // link type submission
                    var linkUrl = new Uri(submission.Url);
                   
                    // add a thumbnail if submission has one
                    if (submission.ThumbnailUrl != null)
                    {
                        //string thumbnailUrl;

                        //if (Settings.UseContentDeliveryNetwork)
                        //{
                        //    thumbnailUrl = new Uri("http://cdn." + System.Web.HttpContext.Current.Request.Url.Authority + "/thumbs/" + submission.Thumbnail).ToString();
                        //}
                        //else
                        //{
                        //    thumbnailUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Thumbs/" + submission.Thumbnail).ToString();
                        //}
                        
                        var item = new SyndicationItem(
                                                submission.Title,
                                                "<a xmlns='http://www.w3.org/1999/xhtml' href='" + commentsUrl + "'><img title='" + submission.Title + "' alt='" + submission.Title + "' src='" + submission.ThumbnailUrl + "' /></a>" +
                                                "</br>" +
                                                "Submitted by " + "<a href='" + authorUrl + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.CommentCount + " comments</a>" +
                                                " | <a href='" + linkUrl + "'>link</a>",
                                                commentsUrl,
                                                submission.ID.ToString(CultureInfo.InvariantCulture),
                                                submission.CreationDate);

                        feedItems.Add(item);
                    }
                    else
                    {
                        var item = new SyndicationItem(
                                                submission.Title,
                                                "Submitted by " + "<a href='" + authorUrl + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.CommentCount + " comments",
                                                commentsUrl,
                                                submission.ID.ToString(CultureInfo.InvariantCulture),
                                                submission.CreationDate);
                        feedItems.Add(item);
                    }
                }
            }

            feed.Items = feedItems;
            return new FeedResult(new Rss20FeedFormatter(feed));
        }
    }
}