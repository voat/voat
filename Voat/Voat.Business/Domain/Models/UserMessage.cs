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
