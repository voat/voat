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
using System.Runtime.Serialization;

namespace Voat.Models.Api.v1
{
    public class ApiSubmission : FormattedContentContainer
    {
        /// <summary>
        /// The submission ID.
        /// </summary>
        [JsonProperty("id")]
        [DataMember(Name = "id")]
        public int ID { get; set; }

        /// <summary>
        /// The number of comments submission current contains.
        /// </summary>
        [JsonProperty("commentCount")]
        [DataMember(Name = "commentCount")]
        public int CommentCount { get; set; }

        /// <summary>
        /// The date the submission was made.
        /// </summary>
        [JsonProperty("date")]
        [DataMember(Name = "date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// The upvote count of the submission.
        /// </summary>
        [JsonProperty("upVotes")]
        [DataMember(Name = "upVotes")]
        public int UpVotes { get; set; }

        /// <summary>
        /// The downvote count of the comment.
        /// </summary>
        [JsonProperty("downVotes")]
        [DataMember(Name = "downVotes")]
        public int DownVotes { get; set; }

        /// <summary>
        /// Date submission was edited.
        /// </summary>
        [JsonProperty("lastEditDate")]
        [DataMember(Name = "lastEditDate")]
        public Nullable<DateTime> LastEditDate { get; set; }

        /// <summary>
        /// The view count of the submission.
        /// </summary>
        [JsonProperty("views")]
        [DataMember(Name = "views")]
        public int Views { get; set; }

        /// <summary>
        /// The user name who submitted the post.
        /// </summary>
        [JsonProperty("userName")]
        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        /// <summary>
        /// The subverse to which this submission belongs.
        /// </summary>
        [JsonProperty("subverse")]
        [DataMember(Name = "subverse")]
        public string Subverse { get; set; }

        /// <summary>
        /// The thumbnail for submission.
        /// </summary>
        [JsonProperty("thumbnail")]
        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }

        /// <summary>
        /// The submission title.
        /// </summary>
        [JsonProperty("title")]
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// The type of submission. Values: 1 for Self Posts, 2 for Link Posts
        /// </summary>
        [JsonProperty("type")]
        [DataMember(Name = "type")]
        public int Type { get; set; }

        /// <summary>
        /// The url for the submission if present.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Include )]
        [DataMember(Name = "url")]
        public string Url { get; set; }

    }

  

   
}