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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Voat.Data.Models;
using Voat.Notifications;

namespace Voat.Utilities.Components
{
    public abstract class ContentFilter
    {
        private ProcessingStage _stage = ProcessingStage.Outbound;
        private int _priority = 0;
        private bool _isReadOnly = false;

        

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
        //InboundPreSave = 1,
        //InboundPostSave = 2,
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
            _replacer = new MatchProcessingReplacer(CONSTANTS.ACCEPTABLE_LEADS + CONSTANTS.USER_HOT_LINK_REGEX, MatchFound) { MatchThreshold = matchThreshold, IgnoreDuplicateMatches = ignoreDuplicatMatches };
        }
       
        protected override string ProcessContent(string content, object context)
        {
            return _replacer.Replace(content, context);
        }
        
        public abstract string MatchFound(Match match, string matchSource, object context);
    }

    //[Obsolete("This should not be part of the content filters", true)]
    //public class UserMentionNotificationFilter : UserMentionFilter
    //{
    //    private class DuplicateUserNameDetectionReplacer : MatchProcessingReplacer
    //    {
    //        public DuplicateUserNameDetectionReplacer(string regEx, Func<Match, string, object, string> replacementFunc) : base(regEx, replacementFunc)
    //        {
    //        }
    //        public override bool IsDuplicate(Match currentMatch, IEnumerable<Match> processedMatches)
    //        {
    //            string groupName = "user";
    //            var found = processedMatches.Any(x => String.Equals(x.Groups[groupName].Value, currentMatch.Groups[groupName].Value, StringComparison.OrdinalIgnoreCase));
    //            return found;
    //        }
    //    }

    //    public UserMentionNotificationFilter()
    //        : base(5, true)
    //    {
    //        _replacer = new DuplicateUserNameDetectionReplacer(_replacer.RegEx, MatchFound) { MatchThreshold = _replacer.MatchThreshold, IgnoreDuplicateMatches = _replacer.IgnoreDuplicateMatches };
    //        Priority = 1;
    //        ProcessingStage = ProcessingStage.InboundPostSave;
    //        IsReadOnly = true;
    //    }
       
    //    public override string MatchFound(Match match, string matchSource, object context)
    //    {
    //        if (!match.Groups["notify"].Success)
    //        {
    //            //Comment mentions
    //            Comment c = context as Comment;
    //            if (c != null && c.LastEditDate == null)
    //            {
    //                NotificationManager.SendUserMentionNotification(match.Groups["user"].Value, c);
    //            }

    //            //Message mentions
    //            Submission m = context as Submission;
    //            if (m != null && m.LastEditDate == null)
    //            {
    //                NotificationManager.SendUserMentionNotification(match.Groups["user"].Value, m);
    //            }
    //        }
    //        return match.Value;
    //    }
    //}

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
            return String.Format("[{0}]({1})", replace, VoatUrlFormatter.UserProfile(match.Groups["user"].Value, new Common.PathOptions(true, true)));
        }
    }
    public class SubverseLinkFilter : ContentFilter
    {
        public SubverseLinkFilter()
        {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 10;
            IsReadOnly = false;

            ProcessLogic = delegate (Match m, string matchSource, object state)
            {
                return String.Format("[{0}]({1})", m.Value, VoatUrlFormatter.Subverse(m.Groups["name"].Value + (m.Groups["fullPath"].Success ? m.Groups["fullPath"].Value : ""), new Common.PathOptions(true, true)));
            };
        }

        protected override string ProcessContent(string content, object context)
        {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(CONSTANTS.ACCEPTABLE_LEADS + CONSTANTS.SUBVERSE_LINK_REGEX_FULL,
               ProcessLogic
            );
            return replacer.Replace(content, context);
        }
    }
    public class SetLinkFilter : ContentFilter
    {
        public SetLinkFilter()
        {
            ProcessingStage = ProcessingStage.Outbound;
            Priority = 10;
            IsReadOnly = false;

            ProcessLogic = delegate (Match m, string matchSource, object state)
            {
                return String.Format("[{0}]({1})", m.Value, VoatUrlFormatter.Set(m.Groups["name"].Value + (m.Groups["fullPath"].Success ? m.Groups["fullPath"].Value : ""), new Common.PathOptions(true, true)));
            };
        }

        protected override string ProcessContent(string content, object context)
        {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(CONSTANTS.ACCEPTABLE_LEADS + CONSTANTS.SET_LINK_REGEX_SHORT,
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
            ProcessLogic = delegate (Match m, string matchSource, object state)
            {
                return String.Format("[{0}](https://np.reddit.com/r/{1})", m.Value, m.Groups["sub"].Value);
            };
        }

        protected override string ProcessContent(string content, object context)
        {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(CONSTANTS.ACCEPTABLE_LEADS + @"((/?r/)(?'sub'[a-zA-Z0-9_]+))", ProcessLogic);
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
            ProcessLogic = delegate (Match m, string matchSource, object state)
            {
                return String.Format("[{0}]({0})", m.Value);
            };
        }

        protected override string ProcessContent(string content, object context)
        {
            MatchProcessingReplacer replacer = new MatchProcessingReplacer(CONSTANTS.ACCEPTABLE_LEADS + CONSTANTS.HTTP_LINK_REGEX + CONSTANTS.ACCEPTABLE_TRAILING,
               ProcessLogic
            );
            return replacer.Replace(content, context);
        }
    }

    #endregion Filter Classes
}
