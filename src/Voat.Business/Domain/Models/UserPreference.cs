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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class UserPreference
    {
        public string UserName { get; set; }

        [Display(Name = "Disable Custom CSS")]
        public bool DisableCSS { get; set; }

        [Display(Name = "Enable Night Mode (Use Dark Theme)")]
        public bool NightMode { get; set; }
        [Display(Name = "Language")]
        public string Language { get; set; }
        [Display(Name = "Open Links in New Window")]
        public bool OpenInNewWindow { get; set; }
        [Display(Name = "Enable Adult (NSFW) Content")]
        public bool EnableAdultContent { get; set; }
        [Display(Name = "Publicly Display Votes")]
        public bool DisplayVotes { get; set; }

        [Display(Name = "Publicly Display Subscriptions")]
        public bool DisplaySubscriptions { get; set; }

        [Display(Name = "Replace Top Bar with Subscriptions")]
        public bool UseSubscriptionsMenu { get; set; }

        [Display(Name = "Publicly Display Votes")]
        public string Bio { get; set; }

        [Display(Name = "Publicly Display Votes")]
        public string Avatar { get; set; }

        [Display(Name = "Display Ads")]
        public bool DisplayAds { get; set; }

        [Display(Name = "Comments to Display")]
        public Nullable<int> DisplayCommentCount { get; set; }

        //[Display(Name = "Highlight Content ")]
        public Nullable<int> HighlightMinutes { get; set; }

        [Display(Name = "Short Title")]
        public string VanityTitle { get; set; }

        [Display(Name = "Vote Threshold For Collapsing Comments")]
        public Nullable<int> CollapseCommentLimit { get; set; }

        [Display(Name = "Block Mentions from Anonymized Subverses")]
        public bool BlockAnonymized { get; set; }

        [Display(Name = "Default Comment Sort")]
        public CommentSortAlgorithm CommentSort { get; set; }

        [Display(Name = "Display Thumbnails")]
        public bool DisplayThumbnails { get; set; }
    }
}
