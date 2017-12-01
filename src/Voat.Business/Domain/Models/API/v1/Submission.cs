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
using System.Collections.Generic;
using System.Linq;

namespace Voat.Domain.Models
{
    public class Submission : VoteableObject
    {
        /// <summary>
        /// The submission ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The subverse to which this submission belongs.
        /// </summary>
        public string Subverse { get; set; }

        /// <summary>
        /// The type of submission. Values: 1 for Self Posts, 2 for Link Posts
        /// </summary>
        public SubmissionType Type { get; set; }

        /// <summary>
        /// The user name who submitted the post.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The submission title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The url for the submission if present.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The thumbnail for submission.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// The raw content of this item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The formatted (MarkDown, Voat Content Processor) content of this item. This content is typically formatted into HTML output.
        /// </summary>
        public string FormattedContent { get; set; }

        /// <summary>
        /// The number of comments submission current contains.
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// The date the submission was made.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Date submission was edited.
        /// </summary>
        public Nullable<DateTime> LastEditDate { get; set; }

        /// <summary>
        /// If submission has a permission set associated with it.
        /// </summary>
        [JsonIgnore]
        public bool HasPermissionSet { get; set; }

        /// <summary>
        /// Is this submission anon
        /// </summary>
        public bool IsAnonymized { get; set; }

        /// <summary>
        /// Is this submission NSFW/Adult rated
        /// </summary>
        public bool IsAdult { get; set; }

        /// <summary>
        /// Is this submission deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The view count of the submission.
        /// </summary>
        public int Views { get; set; }

        public IList<ContentAttribute> Attributes { get; set; } = new List<ContentAttribute>();

        [JsonIgnore]
        public IEnumerable<ContentAttribute> Flairs
        {
            get
            {
                if (Attributes != null && Attributes.Any())
                {
                    return Attributes.Where(x => x.Type == AttributeType.Flair).ToList();
                }
                return Enumerable.Empty<ContentAttribute>();
            }
        }

        //public string FlairLabel { get; set; }

        //public string FlairCss { get; set; }

        [JsonIgnore]
        public double Rank { get; set; }

        [JsonIgnore]
        public double RelativeRank { get; set; }

        /// <summary>
        /// Marker for if current account owns this comment.
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// Date Submission was archived
        /// </summary>
        public DateTime? ArchiveDate { get; set; }

        /// <summary>
        /// The domain of this submission.
        /// </summary>
        public string Domain { get; set; }
    }
}
