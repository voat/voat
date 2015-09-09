using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Models;
using Voat.UI.Utilities;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class RssController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

        // GET: rss/{subverseName}
        public ActionResult Rss(string subverseName)
        {
            var submissions = new List<Submission>();

            if (subverseName != null && subverseName != "all")
            {
                // return only frontpage submissions from a given subverse
                var subverse = DataCache.Subverse.Retrieve(subverseName); // _db.Subverse.Find(subverseName);
                if (subverse != null)
                {

                    //HACK: Disable subverse
                    if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                    {
                        ViewBag.Subverse = subverse.Name;
                        return View("~/Views/Errors/SubverseDisabled.cshtml");
                    }

                    submissions = (from message in _db.Submissions
                                   where !message.IsDeleted && message.Subverse == subverse.Name
                                   select message)
                                   .OrderByDescending(s => s.Rank)
                                   .Take(25)
                                   .ToList();
                }
            }
            else if (subverseName == "all")
            {
                // return submissions from all subs
                submissions = (from message in _db.Submissions
                               join subverse in _db.Subverses on message.Subverse equals subverse.Name
                               where !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && message.Rank > 0.00009
                               where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                               select message).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).Take(25).ToList(); 
            } 
            else
            {
                // return site-wide frontpage submissions
                submissions = (from message in _db.Submissions
                               where !message.IsDeleted
                               join defaultsubverse in _db.DefaultSubverses on message.Subverse equals defaultsubverse.Subverse
                               select message)
                               .OrderByDescending(s => s.Rank)
                               .Take(25)
                               .ToList();
            }

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
                var commentsUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse + "/comments/" + submission.ID);
                var subverseUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse);

                var authorName = submission.UserName;

                if (submission.Type == 1)
                {
                    // submission type submission
                    if (submission.IsAnonymized || submission.Subverse1.IsAnonymized)
                    {
                        authorName = submission.ID.ToString(CultureInfo.InvariantCulture);
                    }

                    var item = new SyndicationItem(
                    submission.Title,
                    submission.Content + "</br>" + "Submitted by " + "<a href='u/" + authorName + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments",
                    commentsUrl,
                    submission.ID.ToString(CultureInfo.InvariantCulture),
                    submission.CreationDate);
                    feedItems.Add(item);
                }
                else
                {
                    // link type submission
                    var linkUrl = new Uri(submission.Content);
                    authorName = submission.UserName;

                    if (submission.IsAnonymized || submission.Subverse1.IsAnonymized)
                    {
                        authorName = submission.ID.ToString(CultureInfo.InvariantCulture);
                    }

                    // add a thumbnail if submission has one
                    if (submission.Thumbnail != null)
                    {
                        string thumbnailUrl;

                        if (Settings.UseContentDeliveryNetwork)
                        {
                            thumbnailUrl = new Uri("http://cdn." + System.Web.HttpContext.Current.Request.Url.Authority + "/thumbs/" + submission.Thumbnail).ToString();
                        }
                        else
                        {
                            thumbnailUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Thumbs/" + submission.Thumbnail).ToString();
                        }
                        
                        var item = new SyndicationItem(
                                                submission.LinkDescription,
                                                "<a xmlns='http://www.w3.org/1999/xhtml' href='" + commentsUrl + "'><img title='" + submission.LinkDescription + "' alt='" + submission.LinkDescription + "' src='" + thumbnailUrl + "' /></a>" +
                                                "</br>" +
                                                "Submitted by " + "<a href='u/" + authorName + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments</a>" +
                                                " | <a href='" + linkUrl + "'>link</a>",
                                                commentsUrl,
                                                submission.ID.ToString(CultureInfo.InvariantCulture),
                                                submission.CreationDate);

                        feedItems.Add(item);
                    }
                    else
                    {
                        var item = new SyndicationItem(
                                                submission.LinkDescription,
                                                "Submitted by " + "<a href='u/" + authorName + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments",
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