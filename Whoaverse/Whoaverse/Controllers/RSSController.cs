using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class RSSController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();
        
        // GET: rss/{subverseName}
        public ActionResult RSS(string subverseName)
        {
            List<Message> submissions = new List<Message>();
            Random rnd = new Random();

            if (subverseName != null)
            {
                // return only frontpage submissions from a given subverse
                Subverse subverse = db.Subverses.Find(subverseName);
                if (subverse != null)
                {
                    submissions = (from message in db.Messages
                                   where message.Name != "deleted" && message.Subverse == subverse.name
                                   select message)
                                   .OrderByDescending(s => s.Rank)
                                   .Take(25)
                                   .ToList();
                }
            }
            else
            {
                // return site-wide frontpage submissions
                submissions = (from message in db.Messages
                               where message.Name != "deleted"
                               join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                               select message)
                               .OrderByDescending(s => s.Rank)
                               .Take(25)
                               .ToList();
            }

            SyndicationFeed feed = new SyndicationFeed("WhoaVerse", "The frontpage of the Universe", new Uri("http://www.whoaverse.com"));
            feed.Language = "en-US";
            feed.ImageUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Graphics/whoaverse_padded.png");

            List<SyndicationItem> feedItems = new List<SyndicationItem>();

            foreach (var submission in submissions)
            {
                var commentsUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse + "/comments/" + submission.Id);
                var subverseUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse);

                string thumbnailUrl = "";
                string authorName = submission.Name;

                if (submission.Type == 1)
                {
                    // message type submission
                    if (submission.Anonymized || submission.Subverses.anonymized_mode)
                    {
                        authorName = submission.Id.ToString();
                    }

                    SyndicationItem item = new SyndicationItem(
                    submission.Title,
                    submission.MessageContent + "</br>" + "Submitted by " + "<a href='u/" + authorName + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments",
                    commentsUrl,
                    "Item ID",
                    submission.Date);
                    feedItems.Add(item);
                }
                else
                {
                    // link type submission
                    var linkUrl = new Uri(submission.MessageContent);
                    authorName = submission.Name;

                    if (submission.Anonymized || submission.Subverses.anonymized_mode)
                    {
                        authorName = submission.Id.ToString();
                    }

                    // add a thumbnail if submission has one
                    if (submission.Thumbnail != null)
                    {
                        thumbnailUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Thumbs/" + submission.Thumbnail).ToString();
                        SyndicationItem item = new SyndicationItem(
                                                submission.Linkdescription,
                                                "<a xmlns='http://www.w3.org/1999/xhtml' href='" + commentsUrl + "'><img title='" + submission.Linkdescription + "' alt='" + submission.Linkdescription + "' src='" + thumbnailUrl + "' /></a>" +
                                                "</br>" +
                                                "Submitted by " + "<a href='u/" + authorName + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments</a>" +
                                                " | <a href='" + linkUrl + "'>link</a>",
                                                commentsUrl,
                                                "Item ID",
                                                submission.Date);

                        feedItems.Add(item);
                    }
                    else
                    {
                        SyndicationItem item = new SyndicationItem(
                                                submission.Linkdescription,
                                                "Submitted by " + "<a href='u/" + authorName + "'>" + authorName + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments",
                                                commentsUrl,
                                                "Item ID",
                                                submission.Date);
                        feedItems.Add(item);
                    }
                }
            }

            feed.Items = feedItems;
            return new FeedResult(new Rss20FeedFormatter(feed));
        }
    }
}