using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Utils.Components {
    public class ContentProcessor {

        // ensure no beforefieldinit - see http://csharpindepth.com/Articles/General/Beforefieldinit.aspx
        static ContentProcessor() { }

        private List<ContentFilter> _filters = new List<ContentFilter>();
        private static Lazy<ContentProcessor> _instance = new Lazy<ContentProcessor>(() => new ContentProcessor
        {
            Filters =
            {
                new UserMentionNotificationFilter(),
                new UserMentionLinkFilter(),
                new SubverseLinkFilter(),
                new RawHyperlinkFilter(),
                new RedditLinkFilter()
            }
        });

        public List<ContentFilter> Filters {
            get { return _filters; }
            set { _filters = value; }
        }

        public static ContentProcessor Instance {
            get
            {
                return _instance.Value;
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