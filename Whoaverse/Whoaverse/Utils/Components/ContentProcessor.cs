using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Utils.Components {
    public class ContentProcessor {

        private List<ContentFilter> _filters = new List<ContentFilter>();
        private static ContentProcessor _instance = null;

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
                                new RedditLinkFilter()});
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
            string c = content;
            var inbound = Filters.FindAll(x => (stage & x.ProcessingStage) > 0).OrderByDescending(x => x.Priority).ToList();
            inbound.ForEach(y => c = y.Process(c, context));
            return c;
        }
        
    }
   

}