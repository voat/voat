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

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Voat.Domain.Models;

namespace Voat.Models.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression("^[a-zA-Z0-9-_]+$")]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters", MinimumLength = 2)]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters", MinimumLength = 6)]
        [RegularExpression("^[^<]+$", ErrorMessage = "The character < is not allowed. Sorry.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }



    public class UserAboutViewModel
    {
        [Display(Name = "Short profile bio")]
        [StringLength(100, ErrorMessage = "The short profile bio is limited to 100 characters.")]
        public string Bio { get; set; }

        public string Avatar { get; set; }

        [Display(Name = "Avatar")]
        public IFormFile Avatarfile { get; set; }
    }

    public class UserEmailViewModel
    {
        [Display(Name = "E-mail address")]
        [EmailAddress(ErrorMessage = "Please enter a valid E-mail address")]
        [StringLength(255, ErrorMessage = "E-mail is limited to a maximum of {1} characters")]
        public string EmailAddress { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required. Please fill this field.")]
        [Display(Name = "User name")]
        [StringLength(50, ErrorMessage = "The {0} must be between {2} and {1} characters", MinimumLength = 1)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "A password is required. Please fill this field.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters", MinimumLength = 2)]
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
        [StringLength(100, ErrorMessage = "{0} must be between {1} and {2} characters in length", MinimumLength = 6)]
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
