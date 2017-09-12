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
using Voat.Common;

namespace Voat.Domain.Models
{

    public enum DeleteOption
    {
        [Display(Name = "Leave As Is")]
        None = 0,
        [Display(Name = "Anonymize")]
        Anonymize = 1,
        [Display(Name = "Delete")]
        Delete = 2
    }


    public class DeleteAccountOptions
    {
        [Required(ErrorMessage = "Please type your UserName")]
        [Display(Name = "UserName")]
        [StringLength(50, ErrorMessage = "{0} is limited to {1} characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Confirm your UserName here")]
        [Display(Name = "Confirm UserName")]
        [Compare("UserName", ErrorMessage = "Confirmation UserName does not match")]
        [StringLength(50, ErrorMessage = "{0} is limited to {1} characters")]
        public string ConfirmUserName { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        [StringLength(50, ErrorMessage = "{0} is limited to {1} characters")]
        public string CurrentPassword { get; set; }

        [Display(Name = "Reason for leaving")]
        [StringLength(500, ErrorMessage = "{0} is limited to {1} characters")]
        public string Reason { get; set; }

        [Display(Name = "Link Submission Action")]
        public SafeEnum<DeleteOption> LinkSubmissions { get; set; } = DeleteOption.None;

        [Display(Name = "Text Submission Action")]
        public SafeEnum<DeleteOption> TextSubmissions { get; set; } = DeleteOption.None;

        [Display(Name = "Comment Action")]
        public SafeEnum<DeleteOption> Comments { get; set; } = DeleteOption.None;

        [EmailAddress]
        [Display(Name = "Recovery Email")]
        [StringLength(255, ErrorMessage = "{0} is limited to a maximum of {1} characters")]
        public string RecoveryEmailAddress { get; set; }

        [EmailAddress]
        [Display(Name = "Confirm Recovery Email")]
        [Compare("RecoveryEmailAddress", ErrorMessage = "Confirmation Recovery Email does not match")]
        [StringLength(50, ErrorMessage = "{0} is limited to {1} characters")]
        public string ConfirmRecoveryEmailAddress { get; set; }
    }
}
