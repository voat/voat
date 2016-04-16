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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Voat.Domain.Models
{
    /// <summary>
    /// Comment information
    /// </summary>
    public class Comment : VoteableObject
    {
        /// <summary>
        /// Child comment count. This is a count of direct decedents only.
        /// </summary>
        public int? ChildCount { get; set; }

        /// <summary>
        /// The raw content of this item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Date comment was submitted.
        /// </summary>
        public DateTime CreationDate { get; set; }

        ///// <summary>
        ///// Level of the comment. 0 is root. This value is relative to the parent comment. If you are loading mid-branch 0 will be returned for the starting position comment.
        ///// </summary>
        //public int? Depth { get; set; }
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
        /// Marker for moderator distinguished comment.
        /// </summary>
        public bool IsDistinguished { get; set; }

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

    /// <summary>
    /// Represents a chunk of comments
    /// </summary>
    public class CommentSegment
    {
        private IList<NestedComment> _comments;

        public CommentSegment()
        {
        }

        public CommentSegment(IList<NestedComment> comments)
        {
            Comments = comments;
        }

        /// <summary>
        /// The list of comments this segment contains
        /// </summary>
        [JsonProperty(Order = 2)]//put on bottom of output
        public IList<NestedComment> Comments
        {
            get
            {
                if (_comments == null)
                {
                    _comments = new List<NestedComment>();
                }
                return _comments;
            }
            set
            {
                _comments = value;
            }
        }

        /// <summary>
        /// The ending index of this comment segment (zero is lowest bound of index)
        /// </summary>
        public int EndingIndex
        {
            get
            {
                return StartingIndex + (SegmentCount == 0 ? 0 : SegmentCount - 1);
            }
        }

        /// <summary>
        /// The count of comments this segment contains
        /// </summary>
        public int SegmentCount
        {
            get
            {
                return (Comments == null ? 0 : Comments.Count());
            }
        }

        /// <summary>
        /// The starting index of this comment segment (zero is lowest bound of index)
        /// </summary>
        public int StartingIndex { get; set; }

        /// <summary>
        /// Represents the total count of comments at this level (root or children of a parent comment).
        /// </summary>
        [JsonProperty(Order = 1)]
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Represents a hierarchical comment tree. This is an experimental class for testing nested comment output via the API.
    /// </summary>
    public class NestedComment : Comment
    {
        /// <summary>
        /// Contains the child comments for this comment.
        /// </summary>
        [JsonProperty(Order = 500)]//put on bottom of output
        public CommentSegment Children { get; set; }

        public void AddChildComment(NestedComment comment)
        {
            if (comment != null)
            {
                if (Children == null)
                {
                    Children = new CommentSegment();
                }
                Children.Comments.Add(comment);
            }
        }
    }
}
