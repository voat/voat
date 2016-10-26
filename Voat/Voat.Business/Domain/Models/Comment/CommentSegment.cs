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
    /// Represents a chunk of comments
    /// </summary>
    public class CommentSegment
    {
        private IList<NestedComment> _comments;
        private int? _startingIndex = null;

        public CommentSegment()
        {
        }

        public CommentSegment(NestedComment comment)
        {
            Comments = new List<NestedComment>() { comment };
        }

        //for backwards compatibility with EF models
        public CommentSegment(Data.Models.Comment comment, string subverse)
        {
            Comments = new List<NestedComment>() { DomainMaps.MapToNestedComment(comment, subverse) };
        }

        public CommentSegment(Comment comment)
        {
            Comments = new List<NestedComment>() { DomainMaps.Map(comment) };
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
        public int StartingIndex {
            get
            {
                if (_startingIndex.HasValue)
                {
                    return _startingIndex.Value;
                }
                else
                {
                    return _comments == null || _comments.Count == 0 ? -1 : 0;
                }
            }
            set
            {
                _startingIndex = value;
            }
        } 

        /// <summary>
        /// The sort order of the comment segment
        /// </summary>
        public CommentSortAlgorithm Sort { get; set; }

        /// <summary>
        /// Represents the total count of comments at this level (root or children of a parent comment)
        /// </summary>
        [JsonProperty(Order = 1)]
        public int TotalCount { get; set; }

        /// <summary>
        /// Returns true if this segment has more records than what this segment includes
        /// </summary>
        public bool HasMore
        {
            get
            {
                return RemainingCount > 0;
            }
        }

        /// <summary>
        /// Returns the remaining record count in this segment
        /// </summary>
        public int RemainingCount
        {
            get
            {
                return Math.Max(0, (TotalCount - (EndingIndex + 1)));
            }
        }
    }
}
