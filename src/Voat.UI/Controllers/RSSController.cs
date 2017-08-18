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

using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Voat.Controllers
{
    public class RssController : BaseController
    {

        // GET: rss/{subverseName}
        public async Task<ActionResult> Rss(string subverseName)
        {
            //CORE_PORT: Not supported
            throw new NotImplementedException("Core port not implemented");
            /*
            subverseName = String.IsNullOrEmpty(subverseName) ? AGGREGATE_SUBVERSE.ALL : subverseName;

            var q = new QuerySubmissions(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, subverseName), new Data.SearchOptions());
            var submissions = await q.ExecuteAsync();

            var feed = new SyndicationFeed("Voat", "Have your say", new Uri("http://www.voat.co"))
            {
                Language = "en-US",
                ImageUrl =
                    new Uri("http://" + System.Web.Context.Request.Url.Authority +
                            "/Graphics/voat-logo.png")
            };

            var feedItems = new List<SyndicationItem>();

            foreach (var submission in submissions)
            {
                var commentsUrl = new Uri("https://" + System.Web.Context.Request.Url.Authority + "/v/" + submission.Subverse + "/comments/" + submission.ID);
                var subverseUrl = new Uri("https://" + System.Web.Context.Request.Url.Authority + "/v/" + submission.Subverse);
                var authorUrl = new Uri("https://" + System.Web.Context.Request.Url.Authority + "/user/" + submission.UserName);

                var authorName = submission.UserName;
                // submission type submission
                if (submission.IsAnonymized)
                {
                    authorName = submission.ID.ToString(CultureInfo.InvariantCulture);
                    authorUrl = new Uri("https://" + System.Web.Context.Request.Url.Authority);
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

                        //if (VoatSettings.Instance.UseContentDeliveryNetwork)
                        //{
                        //    thumbnailUrl = new Uri("http://cdn." + System.Web.Context.Request.Url.Authority + "/thumbs/" + submission.Thumbnail).ToString();
                        //}
                        //else
                        //{
                        //    thumbnailUrl = new Uri("http://" + System.Web.Context.Request.Url.Authority + "/Thumbs/" + submission.Thumbnail).ToString();
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
            */
        }
    }
}
