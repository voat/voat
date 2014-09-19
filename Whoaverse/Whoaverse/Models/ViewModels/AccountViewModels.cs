/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using System.ComponentModel.DataAnnotations;

namespace Whoaverse.Models
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

        //[Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression("^[^<]+$", ErrorMessage = "The character < is not allowed. Sorry.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "New Recovery Question")]
        [StringLength(500, ErrorMessage = "The recovery question must not exceed 500 characters long.")]
        public string NewRecoveryQuestion { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "New Answer")]
        [StringLength(50, ErrorMessage = "Answer must not exceed 50 characters.")]
        public string NewAnswer { get; set; }
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
    }

    public class UserPreferencesViewModel
    {
        [Display(Name = "Disable custom subverse styles")]
        public bool Disable_custom_css { get; set; }

        [Display(Name = "Open links in new tab")]
        public bool OpenLinksInNewTab { get; set; }

        [Display(Name = "Display NSFW content")]
        public bool Enable_adult_content { get; set; }
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
        [StringLength(20, ErrorMessage = "The username should not exceed 20 characters.")]
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

        [DataType(DataType.Text)]
        [Display(Name = "Recovery Question")]
        [StringLength(500, ErrorMessage="Recovery question must not exceed 500 characters.")]
        public string RecoveryQuestion { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Answer")]
        [StringLength(50, ErrorMessage = "Recovery answer must not exceed 50 characters.")]
        public string Answer { get; set; }
    }

    public class PasswordRecoveryModel
    {
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string Question { get; set; }

        [Display(Name = "Answer")]
        public string InputAnswer { get; set; }

        [RegularExpression("^[^<]+$", ErrorMessage = "The character < is not allowed. Sorry.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
