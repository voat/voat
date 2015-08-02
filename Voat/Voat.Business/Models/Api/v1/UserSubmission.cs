using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Voat.Utilities;

namespace Voat.Models.Api.v1
{

    public class UserSubmission {

        /// <summary>
        /// The title for a post. This value is editable within a 10 minute window, afterwards title edits are ignored.
        /// </summary>
        //[Required]
        [MinLength(5)]
        [MaxLength(200)]
        [DescriptionAttribute("The submission title")]
        [DataMember(Name = "title")]
        [JsonProperty("title")]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        public string Title { get; set; }

        /// <summary>
        /// Not Implemented. Specifies if the submission is NSFW or not.
        /// </summary>
        [JsonProperty("nsfw")]
        [DataMember(Name = "nsfw")]
        public bool Nsfw { get; set; }

        /// <summary>
        /// Not Implemented. Specifies if the submission is Anonymous or not.
        /// </summary>
        [JsonProperty("anon")]
        [DataMember(Name = "anon")]
        public bool Anonymous { get; set; }

        /// <summary>
        /// Optional. A value containing the url for a link submission. If this value is set, content is ignored.  Not-Editable once saved.
        /// </summary>
        [Url]
        [JsonProperty("url")]
        [DataMember(Name = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Optional. A value containing the content/text for a submission. Editable for self-posts only.
        /// </summary>
        [MaxLength(10000)]
        [DataMember(Name = "content")]
        [JsonProperty("content")]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        public string Content { get; set; }


        public bool HasState {
            get {
                return (!String.IsNullOrEmpty(Title) || !String.IsNullOrEmpty(Url) || !String.IsNullOrEmpty(Content));
            }
        }
    }
}