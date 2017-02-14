using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string UserName { get; set; }

        [Required(ErrorMessage = "Confirm your UserName here")]
        [Display(Name = "Confirm UserName")]
        [Compare("UserName", ErrorMessage = "Confirmation UserName does not match")]
        public string ConfirmUserName { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Display(Name = "Reason for leaving")]
        public string Reason { get; set; }

        [Display(Name = "Link Submission Action")]
        public DeleteOption LinkSubmissions { get; set; } = DeleteOption.None;

        [Display(Name = "Text Submission Action")]
        public DeleteOption TextSubmissions { get; set; } = DeleteOption.None;

        [Display(Name = "Comment Action")]
        public DeleteOption Comments { get; set; } = DeleteOption.None;

        [EmailAddress]
        [Display(Name = "Recovery Email")]
        public string RecoveryEmailAddress { get; set; }

        [EmailAddress]
        [Display(Name = "Confirm Recovery Email")]
        [Compare("RecoveryEmailAddress", ErrorMessage = "Confirmation Recovery Email does not match")]
        public string ConfirmRecoveryEmailAddress { get; set; }
    }
}
