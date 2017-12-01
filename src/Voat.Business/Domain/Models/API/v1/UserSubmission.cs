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

using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using Voat.Common;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class UserSubmission : UserSubmissionContent
    {
        public UserSubmission()
        {
        }
        public UserSubmission(string subverse, UserSubmissionContent content)
        {
            this.Subverse = subverse;
            this.Title = content.Title;
            this.Url = content.Url;
            this.Content = content.Content;
        }
        
        [Required(ErrorMessage = "A subverse must be provided")]
        public string Subverse { get; set; }

        public override bool IsValid
        {
            get
            {
                return base.IsValid && !String.IsNullOrEmpty(Subverse);
            }
        }
    }

    public class UserSubmissionContent
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

        //[JsonIgnore] CORE_PORT Need this for serialization tests
       

        [JsonIgnore]
        public bool HasState
        {
            get
            {
                return (!String.IsNullOrEmpty(Title) || !String.IsNullOrEmpty(Url) || !String.IsNullOrEmpty(Content));
            }
        }

        [JsonIgnore]
        public virtual bool IsValid
        {
            get
            {
                return (Type == SubmissionType.Link ? !String.IsNullOrEmpty(Url) : true) && !String.IsNullOrEmpty(Title);
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
        /// Specifies if the submission is NSFW or not. If subverse is marked as adult, this setting is overridden
        /// </summary>
        public bool IsAdult { get; set; }

        /// <summary>
        /// Specifies if the submission is Anonymous or not. Used only if subverse allows user defined anonymous posts.
        /// </summary>
        public bool IsAnonymized { get; set; }

        /// <summary>
        /// The title for a post. This value is editable within a 10 minute window, afterwards title edits are ignored.
        /// </summary>
        //[Required]
        [StringLength(200, ErrorMessage = "A title must be between {2} and {1} characters", MinimumLength = 10)]
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
                _title = String.IsNullOrEmpty(value) ? null : value.StripWhiteSpace();
            }
        }

        /// <summary>
        /// Optional. A value containing the url for a link submission. If this value is set, content is ignored.  Not-Editable once saved.
        /// </summary>
        [Url(ErrorMessage = "The url you are trying to submit is invalid")]
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
