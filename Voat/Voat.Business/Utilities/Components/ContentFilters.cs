using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Voat.Data.Models;

namespace Voat.Utilities.Components
{

    public abstract class ContentFilter
    {
        private ProcessingStage _stage = ProcessingStage.InboundPostSave;
        private int _priority = 0;
        private bool _isReadOnly = false;

        public const string ACCEPTABLE_LEADS = @"(?<=\s{1,}|^|\(|\[)";

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }
        public ProcessingStage ProcessingStage
        {
            get { return _stage; }
            set { _stage = value; }
        }

        public virtual Func<Match, string, object, string> ProcessLogic
        {
            get;
            set;
        }

        public virtual string Process(string content, object context)
        {
            string processedContent = ProcessContent(content, context);
            return processedContent;
        }
        protected abstract string ProcessContent(string content, object context);

    }
    [Flags]
    public enum ProcessingStage
    {
        InboundPreSave = 1,
        InboundPostSave = 2,
        Outbound = 4
    }
    public class ContentFilterEventArgs : EventArgs
    {
        public string Content { get; private set; }
        public ContentFilterEventArgs(string content)
        {
            this.Content = content;
        }
    }
    #region Filter Classes

    public abstract class UserMentionFilter : ContentFilter
    {
        protected MatchProcessingReplacer _replacer;
        public UserMentionFilter(int matchThreshold, bool ignoreDuplicatMatches)
        {
            _replacer = new MatchProcessingReplacer(ACCEPTABLE_LEADS + String.Format(@"((?'notify'-)?(?'prefix'@|/u/)(?'user'{0}))", CONSTANTS.USER_NAME_REGEX), MatchFound) { MatchThreshold = matchThreshold, IgnoreDuplicateMatches = ignoreDuplicatMatches };
        }

        protected override string ProcessContent(string content, object context)
        {
            return _replacer.Replace(content, context);
        }

        public abstract string MatchFound(Match match, string matchSource, object context);


    }
    public class UserMentionNotificationFilter : UserMentionFilter
    {
        public UserMentionNotificationFilter()
            : base(5, true)
        {
            
            Priority = 1;
            ProcessingStage = ProcessingStage.InboundPostSave;
            IsReadOnly = true;
        }
        public override string MatchFound(Match match, string matchSource, object context)
        {

            if (!match.Groups["notify"].Success)
            {

                //Comment mentions
                Comment c = context as Comment;
                if (c != null && c.LastEditDate == null)
                {
                    NotificationManager.SendUserMentionNotification(match.Groups["user"].Value, c, ContentProcessor.UserNotificationChanged);
                }
                //Message mentions
                Submission m = context as Submission;
                if (m != null && m.LastEditDate == null)
                {
                    NotificationManager.SendUserMentionNotification(match.Groups["user"].Value, m, ContentProcessor.UserNotificationChanged);
                }
            }
            return match.Value;
        }
    }

    public class UserMentionLinkFilter : UserMentionFilter
    {
        public UserMentionLinkFilter()
            : base(0, false)
        {
            ProcessingStage = ProcessingStage.Outbound;
            IsReadOnly = false;
        }

        public override string MatchFound(Match match, string matchSource, object context)
        {
            string replace = String.Format("{0}{1}", match.Groups["prefix"].Value, match.Groups["user"].Value);
            try
            {
                var u = new UrlHelper(HttpContext.Current.Request.RequestContext, RouteTable.Routes);
                return String.Format("[{0}]({1})", replace, u.Action("UserProfile", "Home", new { id = match.Groups["user"] }));
            }
            catch (Exception ex)
            {
                return String.Format("[{0}](https://voat.co/u/{1})", replace, match.Groups["user"].Value);
            }
        }
    }

    public class SubverseLinkFilter : ContentFilter
    {
        public SubverseLinkFilter()
        {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 10;
            IsReadOnly = false;

            ProcessLogic = delegate(Match m, string matchSource, object state)
            {
                try
                {
                    var u = new UrlHelper(HttpContext.Current.Request.RequestContext, RouteTable.Routes);
                    return String.Format("[{0}]({1})", m.Value, u.Action("SubverseIndex", "Subverses", new { subversetoshow = m.Groups["sub"] }) + (m.Groups["anchor"].Success ? m.Groups["anchor"].Value : ""));
                }
                catch (Exception ex)
                {
                    return String.Format("[{0}](https://voat.co/v/{1})", m.Value, m.Groups["sub"].Value + (m.Groups["anchor"].Success ? m.Groups["anchor"].Value : ""));
                }
            };


        }
        protected override string ProcessContent(string content, object context)
        {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(ACCEPTABLE_LEADS + @"((/?v/)(?'sub'[a-zA-Z0-9]+((/(new|top(\?time=(day|week|month|year|all))?|comments/\d+(/\d+(?:/\d+(?:\d+)?)?)?)))?)(?'anchor'#(?:\d+|submissionTop))?)",
               ProcessLogic
            );
            return replacer.Replace(content, context);
        }
    }

    public class RedditLinkFilter : ContentFilter
    {
        public RedditLinkFilter()
        {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 10;
            IsReadOnly = false;
            ProcessLogic = delegate(Match m, string matchSource, object state)
            {
                return String.Format("[{0}](https://np.reddit.com/r/{1})", m.Value, m.Groups["sub"].Value);
            };

        }
        protected override string ProcessContent(string content, object context)
        {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(ACCEPTABLE_LEADS + @"((/?r/)(?'sub'[a-zA-Z0-9_]+))", ProcessLogic);
            return replacer.Replace(content, context);
        }
    }
    public class RawHyperlinkFilter : ContentFilter
    {
        public RawHyperlinkFilter()
        {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 0;
            IsReadOnly = false;
            ProcessLogic = delegate(Match m, string matchSource, object state)
            {
                return String.Format("[{0}]({0})", m.Value);
            };
        }

        protected override string ProcessContent(string content, object context)
        {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(ACCEPTABLE_LEADS + CONSTANTS.HTTP_LINK_REGEX,
               ProcessLogic
            );
            return replacer.Replace(content, context);
        }
    }
    #endregion
}
