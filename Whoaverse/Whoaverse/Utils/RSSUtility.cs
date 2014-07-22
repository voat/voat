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
using System.Web;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.ServiceModel.Syndication;

namespace Whoaverse.Utils
{
    /* special thanks to Damien Guard for writing an article on Creating RSS feeds in ASP.NET MVC     
     * which helped me understand how to work with RSS in ASP.NET MVC
     * article was published at: http://damieng.com/blog/2010/04/26/creating-rss-feeds-in-asp-net-mvc
     * Atko
     */
    public class FeedResult : ActionResult
    {
        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }

        private readonly SyndicationFeedFormatter feed;
        public SyndicationFeedFormatter Feed
        {
            get { return feed; }
        }

        public FeedResult(SyndicationFeedFormatter feed)
        {
            this.feed = feed;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/rss+xml";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (feed != null)
                using (var xmlWriter = new XmlTextWriter(response.Output))
                {
                    xmlWriter.Formatting = System.Xml.Formatting.Indented;
                    feed.WriteTo(xmlWriter);
                }
        }
    }
}