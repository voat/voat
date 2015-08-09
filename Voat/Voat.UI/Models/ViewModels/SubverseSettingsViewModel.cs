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

using System;
using System.ComponentModel.DataAnnotations;

namespace Voat.Models.ViewModels
{
    public class SubverseSettingsViewModel
    {
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter a description.")]
        [StringLength(500, ErrorMessage = "The description is limited to 500 characters.")]
        public string Description { get; set; }

        [StringLength(50000, ErrorMessage = "The stylesheet is limited to 50000 characters.")]
        public string Stylesheet { get; set; }

        [Required(ErrorMessage = "Please enter a sidebar text.")]
        [StringLength(4000, ErrorMessage = "The sidebar text is limited to 4000 characters.")]
        public string Sidebar { get; set; }

        [StringLength(500, ErrorMessage = "The submission text is limited to 1024 characters.")]
        public string Submission_text { get; set; }

        [StringLength(10, ErrorMessage = "The subverse type limited to 10 characters.")]
        [RegularExpression("link|self", ErrorMessage = "Please type link for link and self posts only or, type self for self posts only.")]
        public string Type { get; set; }

        [StringLength(50, ErrorMessage = "The label for new link submissions is limited to 50 characters.")]
        public string Label_submit_new_link { get; set; }

        [StringLength(50, ErrorMessage = "The label for new self submissions is limited to 50 characters.")]
        public string Label_sumit_new_selfpost { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Rated_adult { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Allow_default { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Private_subverse { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Enable_thumbnails { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Exclude_sitewide_bans { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Authorized_submitters_only { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Anonymized_mode { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        [Range(0, 10000, ErrorMessage = "Minimum CCP value must be between 0 and 10000")]
        public int Minimumdownvoteccp { get; set; }

    }
}