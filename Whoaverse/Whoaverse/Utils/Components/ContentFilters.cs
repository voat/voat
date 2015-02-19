using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Voat.Models;

namespace Voat.Utils.Components {

    public abstract class ContentFilter {
        private ProcessingStage _stage = ProcessingStage.InboundPostSave;
        private int _priority = 0;
        private bool _isReadOnly = false;

        public bool IsReadOnly {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        public int Priority {
            get { return _priority; }
            set { _priority = value; }
        }
        public ProcessingStage ProcessingStage {
            get { return _stage; }
            set { _stage = value; }
        }
        public virtual string Process(string content, object context) {
            string processedContent = ProcessContent(content, context);
            return processedContent;
        }
        protected abstract string ProcessContent(string content, object context);

    }
    [Flags]
    public enum ProcessingStage {
        InboundPreSave = 1,
        InboundPostSave = 2,
        Outbound = 4
    }
    public class ContentFilterEventArgs : EventArgs {
        public string Content { get; private set; }
        public ContentFilterEventArgs(string content) {
            this.Content = content;
        }
    }
    #region Filter Classes
    public abstract class UserMentionFilter : ContentFilter { 
        protected RegExReplacer _replacer;
        public UserMentionFilter(int matchThreshold, bool ignoreDuplicatMatches){
            _replacer = new RegExReplacer();
            // -> @user & /u/user
            _replacer.Replacers.Add(new MatchProcessingReplacer(@"(?<=\s{1,}|^|\()((@|/u/)(?'user'[a-zA-Z0-9-_]+))", MatchFound) { MatchThreshold = matchThreshold, IgnoreDuplicateMatches = ignoreDuplicatMatches });
        }

        protected override string ProcessContent(string content, object context) {
            return _replacer.Replace(content, context);
        }

        public abstract string MatchFound(Match match, object context);

    
    }
    public class UserMentionNotificationFilter : UserMentionFilter {
        public UserMentionNotificationFilter() : base(5, true) {
            Priority = 1;
            ProcessingStage = ProcessingStage.InboundPostSave;
            IsReadOnly = true;
        }
        public override string MatchFound(Match match, object context) {
            //Comment mentions
            Comment c = context as Comment;
            if (c != null && c.LastEditDate == null) {
                NotificationManager.SendUserMentionNotification(match.Groups["user"].Value, c);
            }
            //Message mentions
            Message m = context as Message;
            if (m != null && m.LastEditDate == null) {
                NotificationManager.SendUserMentionNotification(match.Groups["user"].Value, m);
            }
            return match.Value;
        }
    }

    public class UserMentionLinkFilter : UserMentionFilter {
        public UserMentionLinkFilter() : base(0, false) {
            ProcessingStage = ProcessingStage.Outbound;
            IsReadOnly = false;
        }

        public override string MatchFound(Match match, object context) {
            var u = new UrlHelper(HttpContext.Current.Request.RequestContext, RouteTable.Routes);
            return String.Format("[{0}]({1})", match.Value, u.Action( "UserProfile", "Home", new { id = match.Groups["user"] }));
        }
    }
    
    public class SubverseLinkFilter : ContentFilter {
        public SubverseLinkFilter() {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 10;
            IsReadOnly = false;

        }

        protected override string ProcessContent(string content, object context) {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(@"(?<=\s{1,}|^)((/v/)(?'sub'[a-zA-Z0-9]+))", 
                delegate(Match m, object state) {
                    var u = new UrlHelper(HttpContext.Current.Request.RequestContext, RouteTable.Routes);
                    return String.Format("[{0}]({0})", u.Action("SubverseIndex", "Subverses", new { subversetoshow = m.Groups["sub"] }));
                }
            );
            return replacer.Replace(content, context);
        }
    }

    public class RedditLinkFilter : ContentFilter {
        public RedditLinkFilter() {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 10;
            IsReadOnly = false;

        }
        protected override string ProcessContent(string content, object context) {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(@"(?<=\s{1,}|^)((/r/)(?'sub'[a-zA-Z0-9]+))",
                delegate(Match m, object state) {
                    return String.Format("[{0}](http://np.reddit.com{0})", m.Value);
                }
            );
            return replacer.Replace(content, context);
        }
    }
     public class RawHyperlinkFilter : ContentFilter {
        public RawHyperlinkFilter() {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 0;
            IsReadOnly = false;

        }

        protected override string ProcessContent(string content, object context) {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(@"(?<!\(\s{1,}|\[\s{1,})(?<=\s{1,}|^)(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=]*)(?<![\.\?\-_\,]|\s{1,})", 
                delegate(Match m, object state) {
                    return String.Format("[{0}]({0})",m.Value);
                }
            );
            return replacer.Replace(content, context);
        }
    }
    #endregion


}