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
using System.Web.Mvc;

namespace Voat.Models
{
    public class MarkdownPreviewModel
    {
        [Required(ErrorMessage = "Comment text is required. Please fill this field.")]
        [StringLength(10000, ErrorMessage = "Comment text is limited to 10.000 characters.")]
        [AllowHtml]
        public string MessageContent { get; set; }
    }
}
