using System;

namespace Voat.Domain.Models
{
    public class UserMessage
    {
        /// <summary>
        /// The CommentID that this message is related.
        /// </summary>
        public int? CommentID { get; set; }

        /// <summary>
        /// The raw content of this item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The formatted (MarkDown, Voat Content Processor) content of this item. This content is typically formatted into HTML output.
        /// </summary>
        public string FormattedContent { get; set; }

        /// <summary>
        /// The ID of the message
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The entity that message was sent to.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// The entity that sent message.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Date message was sent.
        /// </summary>
        public DateTime SentDate { get; set; }

        /// <summary>
        /// The Subject of the message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The SubmissionID that this message is related.
        /// </summary>
        public int? SubmissionID { get; set; }

        /// <summary>
        /// The subverse that this message is related.
        /// </summary>
        public string Subverse { get; set; }

        /// <summary>
        /// The type of message
        /// </summary>
        public MessageTypeFlag Type { get; set; }

        /// <summary>
        /// A flag regarding the read state of message.
        /// </summary>
        public bool IsRead { get; set; }
    }
}
