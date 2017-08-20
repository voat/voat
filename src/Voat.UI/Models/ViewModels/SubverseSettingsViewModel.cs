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
using System.ComponentModel.DataAnnotations;


namespace Voat.Models.ViewModels
{
    public class SubverseSettingsViewModel
    {
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "The title is limited to 100 characters.")]
        [Display(Name="Short Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please enter a description.")]
        [StringLength(500, ErrorMessage = "The description is limited to 500 characters.")]
        public string Description { get; set; }

        [StringLength(50000, ErrorMessage = "The stylesheet is limited to 50,000 characters.")]
        public string Stylesheet { get; set; }

        [Required(ErrorMessage = "Please enter a sidebar text.")]
        [StringLength(4000, ErrorMessage = "The sidebar text is limited to 4,000 characters.")]
        //CORE_PORT: Not Supported
        //[AllowHtml]
        public string SideBar { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool IsAdult { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool IsPrivate { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool IsThumbnailEnabled { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool ExcludeSitewideBans { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool IsAuthorizedOnly { get; set; }

        //[Required(ErrorMessage = "This setting is required.")]
        public bool? IsAnonymized { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        [Range(0, 10000, ErrorMessage = "Minimum CCP value must be between 0 and 10,000")]
        public int MinCCPForDownvote { get; set; }

        public DateTime? LastUpdateDate { get; set; }
    }
}
