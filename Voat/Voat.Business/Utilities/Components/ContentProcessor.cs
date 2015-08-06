using System;
using System.Collections.Generic;
using System.Linq;

namespace Voat.Utilities.Components
{
    public class ContentProcessor {

        private List<ContentFilter> _filters = new List<ContentFilter>();
        private static ContentProcessor _instance = null;

        //HACK: Need to signal UI that a users notification count changes. This is a dirty way to accomplish this as a workaround. 
        public static Action<string> UserNotificationChanged;



        public List<ContentFilter> Filters {
            get { return _filters; }
            set { _filters = value; }
        }

        public static ContentProcessor Instance {
            get {
                if (_instance == null) {
                    lock (typeof(ContentProcessor)) {
                        if (_instance == null) {
                            var p = new ContentProcessor();
                            p.Filters.AddRange(new ContentFilter[] {
                                new UserMentionNotificationFilter(), 
                                new UserMentionLinkFilter(),
                                new SubverseLinkFilter(),
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
        public bool HasStage(ProcessingStage stage) {
            return Filters.Exists(x => (stage & x.ProcessingStage) > 0);
        }
        public string Process(string content, ProcessingStage stage, object context) {
            if (content == null) {
                return content;
            }
            string c = content;
            var inbound = Filters.FindAll(x => (stage & x.ProcessingStage) > 0).OrderByDescending(x => x.Priority).ToList();
            inbound.ForEach(y => c = y.Process(c, context));
            return c;
        }
        
    }
   

}