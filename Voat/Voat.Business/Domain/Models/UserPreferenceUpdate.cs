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
    }
}
