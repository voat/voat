using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class UserSubmission
    {
        /// <summary>
        /// Optional. A value containing the content/text for a submission. Editable for self-posts only.
        /// </summary>
        [MaxLength(10000)]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        public string Content { get; set; }

        [JsonIgnore]
        public string Subverse { get; set; }

        [JsonIgnore]
        public bool HasState
        {
            get
            {
                return (!String.IsNullOrEmpty(Title) || !String.IsNullOrEmpty(Url) || !String.IsNullOrEmpty(Content));
            }
        }

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                return (Type == SubmissionType.Link ? !String.IsNullOrEmpty(Url) : true) && !String.IsNullOrEmpty(Title) && !String.IsNullOrEmpty(Subverse);
            }
        }
        [JsonIgnore]
        public SubmissionType Type
        {
            get
            {
                return String.IsNullOrEmpty(Url) ? SubmissionType.Text : SubmissionType.Link;
            }
        }


        /// <summary>
        /// Not Implemented. Specifies if the submission is NSFW or not.
        /// </summary>
        public bool IsAdult { get; set; }

        /// <summary>
        /// Not Implemented. Specifies if the submission is Anonymous or not.
        /// </summary>
        public bool IsAnonymized { get; set; }

        /// <summary>
        /// The title for a post. This value is editable within a 10 minute window, afterwards title edits are ignored.
        /// </summary>
        //[Required]
        [MinLength(5)]
        [MaxLength(200)]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        public string Title { get; set; }

        /// <summary>
        /// Optional. A value containing the url for a link submission. If this value is set, content is ignored.  Not-Editable once saved.
        /// </summary>
        [Url]
        public string Url { get; set; }
    }
}
