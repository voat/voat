using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Voat.Models.Api {

    public abstract class FormattedContentContainer {

        private string _content = null;

        /// <summary>
        /// The raw content of this item.
        /// </summary>
        [JsonProperty("content", NullValueHandling = NullValueHandling.Include)]
        [DataMember(Name = "content")]
        public string Content {
            get {
                return _content;
            }
            set {
                _content = value;
                try {
                    FormattedContent = Utilities.Formatting.FormatMessage(_content);
                } catch { }
            }
        }

        /// <summary>
        /// The formatted (MarkDown, Voat Content Processor) content of this item. This content is typically formatted into HTML output.
        /// </summary>
        [JsonProperty("formattedContent", NullValueHandling = NullValueHandling.Include)]
        [DataMember(Name = "formattedContent")]
        public string FormattedContent { get; set; }
    }
}