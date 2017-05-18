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
using System.Web;

namespace Voat.Models.ViewModels
{
    public class ApiKeyCreateRequest
    {

        public string ID { get; set; }

        [Display(Name = "App Name")]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "App Description")]
        [StringLength(2000)]
        public string Description { get; set; }

        [Display(Name = "App About Url")]
        [StringLength(200)]
        [Url(ErrorMessage = "Please enter a valid http, https, or ftp URL.")]
        public string AboutUrl { get; set; }


        [Display(Name = "OAuth Redirect Url")]
        [StringLength(500)]
        [RegularExpression(Utilities.CONSTANTS.URI_LINK_REGEX_UI, ErrorMessage = "URI must conform to standard protocols")]
        public string RedirectUrl { get; set; }
        
    }
}
