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

namespace Voat.Utilities.Components
{
    public class ContentProcessor
    {
        private List<ContentFilter> _filters = new List<ContentFilter>();
        private static ContentProcessor _instance = null;

        ////HACK: Need to signal UI that a users notification count changes. This is a dirty way to accomplish this as a workaround.
        //public static Action<string> UserNotificationChanged;

        public List<ContentFilter> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        public static ContentProcessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(ContentProcessor))
                    {
                        if (_instance == null)
                        {
                            var p = new ContentProcessor();
                            p.Filters.AddRange(new ContentFilter[] {
                                //new UserMentionNotificationFilter(),
                                new UserMentionLinkFilter(),
                                new SubverseLinkFilter(),
                                new SetLinkFilter(),
                                new RawHyperlinkFilter(),
                                new RedditLinkFilter()
                            });
                            _instance = p;
                        }
                    }
                }
                return _instance;
            }
        }

        public bool HasStage(ProcessingStage stage)
        {
            return Filters.Exists(x => (stage & x.ProcessingStage) > 0);
        }

        public string Process(string content, ProcessingStage stage, object context)
        {
            if (content == null)
            {
                return content;
            }
            string c = content;
            var inbound = Filters.FindAll(x => (stage & x.ProcessingStage) > 0).OrderByDescending(x => x.Priority).ToList();
            inbound.ForEach(y => c = y.Process(c, context));
            return c;
        }
    }
}
