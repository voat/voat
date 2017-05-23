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

using System.ComponentModel.DataAnnotations;

namespace Voat.Models.ViewModels
{
    public class SubverseBanViewModel
    {
        [Required(ErrorMessage = "Please enter a username.")]
        [StringLength(23, ErrorMessage = "Username is limited to 23 characters.")]
        public string UserName { get; set; }

        public string Subverse { get; set; }

        [Required(ErrorMessage = "Please enter a ban reason.")]
        [StringLength(500, ErrorMessage = "Ban reason is limited to 500 characters.")]
        public string Reason { get; set; }
    }
}
