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

using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Voat.Models.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression("^[a-zA-Z0-9-_]+$")]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression("^[^<]+$", ErrorMessage = "The character < is not allowed. Sorry.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class DeleteAccountViewModel
    {
        [Required(ErrorMessage = "Please type the word DELETE in this field.")]        
        [RegularExpression("DELETE", ErrorMessage = "Please type the word DELETE in this field.")]
        [Display(Name = "Type DELETE to confirm")]
        public string FirstWord { get; set; }

        [Required(ErrorMessage = "Please type the word DELETE in this field.")]
        [RegularExpression("DELETE", ErrorMessage = "Please type the word DELETE in this field.")]
        [Display(Name = "Re-type DELETE to confirm")]
        [Compare("FirstWord", ErrorMessage = "Please re-type the word DELETE in this field.")]
        public string SecondWord { get; set; }

        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }
    }

    public class UserPreferencesViewModel
    {
        [Display(Name = "Disable custom subverse styles")]
        public bool Disable_custom_css { get; set; }
        
        [Display(Name = "Enable night mode (use dark theme)")]
        public bool Night_mode { get; set; }

        [Display(Name = "Open links in new tab")]
        public bool OpenLinksInNewTab { get; set; }

        [Display(Name = "Display NSFW content")]
        public bool Enable_adult_content { get; set; }

        [Display(Name = "Publicly display my subscriptions on my profile")]
        public bool Public_subscriptions { get; set; }

        [Display(Name = "Replace top menu bar with my subscriptions")]
        public bool Topmenu_from_subscriptions { get; set; }
    }

    public class UserAboutViewModel
    {
        [Display(Name = "Short profile bio")]
        [StringLength(100, ErrorMessage = "The short profile bio is limited to 100 characters.")]
        public string Bio { get; set; }

        public string Avatar { get; set; }

        [Display(Name = "Avatar")]
        public HttpPostedFileBase Avatarfile { get; set; }
    }

    public class UserEmailViewModel
    {
        [Display(Name = "E-mail address")]
        [EmailAddress(ErrorMessage = "Please enter a valid E-mail address")]
        [Required(ErrorMessage = "E-mail address is required. Please fill this field.")]
        public string EmailAddress { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required. Please fill this field.")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "A password is required. Please fill this field.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [RegularExpression(@"^[a-zA-Z0-9][A-Za-z0-9-_]*$", ErrorMessage="The username must be alphanumeric and start with a letter or number. It may contain hyphens and underscores.")]
        [Required(ErrorMessage = "Username is required. Please fill this field.")]
        [StringLength(20, ErrorMessage = "The username should not exceed 20 characters and be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "A password is required. Please fill this field.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]        
        [RegularExpression("^[^<]+$", ErrorMessage = "The character < is not allowed. Sorry.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
