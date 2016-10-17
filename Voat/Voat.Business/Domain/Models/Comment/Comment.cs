/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;

namespace Voat.Domain.Models
{
    /// <summary>
    /// Comment information
    /// </summary>
    public class Comment : VoteableObject
    {
        /// <summary>
        /// The raw content of this item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Date comment was submitted.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The formatted (MarkDown, Voat Content Processor) content of this item. This content is typically formatted into HTML output.
        /// </summary>
        public string FormattedContent { get; set; }

        /// <summary>
        /// The comment ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Marker for anon comment.
        /// </summary>
        public bool IsAnonymized { get; set; }

        /// <summary>
        /// Is this comment below the viewing threshold for the user.
        /// </summary>
        public bool IsCollapsed { get; set; }

        /// <summary>
        /// Marker for deleted comment.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Marker for saved comment.
        /// </summary>
        public bool? IsSaved { get; set; }

        /// <summary>
        /// Marker for moderator distinguished comment.
        /// </summary>
        public bool IsDistinguished { get; set; }

        /// <summary>
        /// Marker for if current account owns this comment.
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// Marker for if comment belongs to OP.
        /// </summary>
        public bool IsSubmitter { get; set; }

        /// <summary>
        /// Date comment was edited.
        /// </summary>
        public Nullable<DateTime> LastEditDate { get; set; }

        /// <summary>
        /// The parent comment ID. If null then comment is a root comment.
        /// </summary>
        public Nullable<int> ParentID { get; set; }

        /// <summary>
        /// The submission ID that this comment belongs.
        /// </summary>
        public Nullable<int> SubmissionID { get; set; }

        /// <summary>
        /// The subveres that this comment belongs.
        /// </summary>
        public string Subverse { get; set; }

        /// <summary>
        /// The user name who submitted the comment.
        /// </summary>
        public string UserName { get; set; }
    }
}
