using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Voat.Models.Api;

namespace Voat.Models
{

    public class UserMessage : FormattedContentContainer
    {

        /// <summary>
        /// The ID of the message
        /// </summary>
        [JsonProperty("id")]
        [DataMember(Name = "id")]
        public int ID { get; set; }

        /// <summary>
        /// The CommentID that this message is related.
        /// </summary>
        [JsonProperty("commentID")]
        [DataMember(Name = "commentID")]
        public int? CommentID { get; set; }

        /// <summary>
        /// The SubmissionID that this message is related.
        /// </summary>
        [JsonProperty("submissionID")]
        [DataMember(Name = "submissionID")]
        public int? SubmissionID { get; set; }

        /// <summary>
        /// The subverse that this message is related.
        /// </summary>
        [JsonProperty("subverse")]
        [DataMember(Name = "subverse")]
        public string Subverse { get; set; }

        /// <summary>
        /// The entity that message was sent to.
        /// </summary>
        [JsonProperty("recipient")]
        [DataMember(Name = "recipient")]
        public string Recipient { get; set; }

        /// <summary>
        /// The entity that sent message.
        /// </summary>
        [JsonProperty("sender")]
        [DataMember(Name = "sender")]
        public string Sender { get; set; }

        /// <summary>
        /// The Subject of the message.
        /// </summary>
        [JsonProperty("subject")]
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// A flag regarding the read state of message.
        /// </summary>
        [JsonProperty("unread")]
        [DataMember(Name = "unread")]
        public bool Unread { get; set; }

        /// <summary>
        /// The type of message
        /// </summary>
        [JsonProperty("type")]
        [DataMember(Name = "type")]
        public MessageType Type { get; set; }

        /// <summary>
        /// The text description of the type of message this is.
        /// </summary>
        [DataMember(Name = "typeName")]
        [JsonProperty("typeName")]
        public string TypeName
        {
            get
            {
                return this.Type.ToString();
            }
        }

        /// <summary>
        /// Date message was sent.
        /// </summary>
        [JsonProperty("sentDate")]
        [DataMember(Name = "sentDate")]
        public DateTime SentDate { get; set; }
    }
}
