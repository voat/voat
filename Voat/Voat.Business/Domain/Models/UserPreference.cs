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

        [Display(Name = "Replace Top Bar with Subscription")]
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
    }
}
