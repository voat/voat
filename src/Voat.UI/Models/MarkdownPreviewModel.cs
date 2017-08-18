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

namespace Voat.Models
{
    public class MarkdownPreviewModel
    {
        [Required(ErrorMessage = "Text is required. Please fill this field.")]
        [StringLength(10000, ErrorMessage = "Text is limited to 10000 characters.")]
        //CORE_PORT: Not Supported
        //[AllowHtml]
        public string MessageContent { get; set; }
    }
}
