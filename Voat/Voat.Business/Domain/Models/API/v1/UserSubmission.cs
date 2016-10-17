using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class UserSubmission
    {
        private string _title = null;
        private string _url = null;
        private string _content = null;

        /// <summary>
        /// Optional. A value containing the content/text for a submission. Editable for self-posts only.
        /// </summary>
        [MaxLength(10000)]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        public string Content
        {
            get
            {
                return _content;
            }

            set
            {
                //string whitespace
                _content = String.IsNullOrEmpty(value) ? "" : value.Trim();
            }
        }

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
        [MinLength(10)]
        [MaxLength(200)]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                //string whitespace
                _title = String.IsNullOrEmpty(value) ? null : Utilities.Formatting.StripWhiteSpace(value);
            }
        }

        /// <summary>
        /// Optional. A value containing the url for a link submission. If this value is set, content is ignored.  Not-Editable once saved.
        /// </summary>
        [Url]
        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                //string whitespace
                _url = String.IsNullOrEmpty(value) ? null : value.Trim();
            }
        }
    }
}
