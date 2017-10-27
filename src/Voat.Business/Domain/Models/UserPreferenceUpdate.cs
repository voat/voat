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

using System.ComponentModel.DataAnnotations;
using Voat.Common;

namespace Voat.Domain.Models
{
    public class UserPreferenceUpdate
    {
        public bool? DisableCSS { get; set; }

        public bool? NightMode { get; set; }

        [StringLength(50)]
        public string Language { get; set; }

        public bool? OpenInNewWindow { get; set; }

        public bool? EnableAdultContent { get; set; }

        public bool? DisplayVotes { get; set; }

        public bool? DisplaySubscriptions { get; set; }

        public bool? UseSubscriptionsMenu { get; set; }

        [StringLength(100)]
        public string Bio { get; set; }

        //public string Avatar { get; set; }
        //public string UserName { get; set; }
        public bool? DisplayAds { get; set; }

        public int? DisplayCommentCount { get; set; }

        public int? HighlightMinutes { get; set; }

        [StringLength(50)]
        public string VanityTitle { get; set; }

        public int? CollapseCommentLimit { get; set; }

        /// <summary>
        /// When this setting is true, mentions and pings from submissions and comments that are Anonymized are not sent to the user.
        /// </summary>
        public bool? BlockAnonymized { get; set; }

        /// <summary>
        /// Specifies the default sort used for comments when no sort is supplied.
        /// </summary>
        public SafeEnum<CommentSortAlgorithm> CommentSort { get; set; }

        public bool? DisplayThumbnails { get; set; }
    }
}
