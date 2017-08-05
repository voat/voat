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

namespace Voat.Models
{
    public class AddSubverse
    {

        //any upper or lower case alphabetic or numeric character, no spaces or special characters, length: 20
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Sorry, no spaces or special characters, max 20 characters")]
        [StringLength(20, ErrorMessage = "The name length is limited to 20 characters")]
        [Required(ErrorMessage = "Name is required. Seriously.")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "The title length is limited to 100 characters")]
        [Display(Name="Short Title")]
        public string Title { get; set; }

        [StringLength(4000, ErrorMessage = "The Sidebar length is limited to 4,000 characters")]
        public string Sidebar { get; set; }
 
        [Required(ErrorMessage = "Please describe what your subverse is about")]
        [StringLength(500, ErrorMessage = "The description length is limited to 500 characters")]
        public string Description { get; set; }
        
    }
}
