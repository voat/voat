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

namespace Voat.Domain.Models
{
    /// <summary>
    /// Represents a hierarchical comment tree. This is an experimental class for testing nested comment output via the API.
    /// </summary>
    public class NestedComment : Comment
    {
        /// <summary>
        /// Child comment count. This is a count of direct decedents only.
        /// </summary>
        public int ChildCount { get; set; }

        /// <summary>
        /// Contains the child comments for this comment.
        /// </summary>
        [JsonProperty(Order = 500)]//put on bottom of output
        public CommentSegment Children { get; set; } = new CommentSegment();

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
