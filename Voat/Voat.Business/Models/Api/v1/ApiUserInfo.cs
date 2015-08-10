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
using System.Runtime.Serialization;
using Voat.Common;

namespace Voat.Models.Api.v1
{
    public class ApiUserInfo
    {
        ///// <summary>
        ///// Path of avatar file
        ///// </summary>
        //[JsonProperty("avatar")]
        //[DataMember(Name = "avatar")]
        //public string Avatar { get; set; }

        /// <summary>
        /// The user name of the user when addressed by name
        /// </summary>
        [JsonProperty("userName")]
        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Date user registered
        /// </summary>
        [JsonProperty("registrationDate")]
        [DataMember(Name = "registrationDate")]
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Short bio of user
        /// </summary>
        [JsonProperty("bio")]
        [DataMember(Name = "bio")]
        public string Bio { get; set; }

        /// <summary>
        /// Path to profile picture
        /// </summary>
        [JsonProperty("profilePicture")]
        [DataMember(Name = "profilePicture")]
        public string ProfilePicture { get; set; }
        
        ///// <summary>
        ///// Comment Contribution Points
        ///// </summary>
        //public int CCP { get; set; }

        ///// <summary>
        ///// Submission Contribution Points
        ///// </summary>
        //public int SCP { get; set; }


        /// <summary>
        /// Comment Contribution Points (CCP)
        /// </summary>
        [JsonProperty("commentPoints")]
        [DataMember(Name = "commentPoints")]
        public Score CommentPoints { get; set; }

        /// <summary>
        /// Submission Contribution Points (SCP)
        /// </summary>
        [JsonProperty("submissionPoints")]
        [DataMember(Name = "submissionPoints")]
        public Score SubmissionPoints { get; set; }

        /// <summary>
        /// Comment Voting Behavior (Only available if request is authenticated)
        /// </summary>
        [JsonProperty("commentVoting")]
        [DataMember(Name = "commentVoting")]
        public Score CommentVoting { get; set; }

        /// <summary>
        /// Submission Voting Distribution (Only available if request is authenticated)
        /// </summary>
        [JsonProperty("submissionVoting")]
        [DataMember(Name = "submissionVoting")]
        public Score SubmissionVoting { get; set; }

        /// <summary>
        /// The badges the user has accumulated 
        /// </summary>
        [JsonProperty("badges")]
        [DataMember(Name = "badges")]
        public List<ApiUserBadge> Badges { get; set; }
    }
}