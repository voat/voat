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
using Voat.Common;
using Voat.Utilities;

namespace Voat.Models.ViewModels
{
    public class ModeratorDeleteContentViewModel
    {
        private string _reason = "";
        [Required(ErrorMessage = "Expecting an ID")]

        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter a deletion reason")]
        [StringLength(500, ErrorMessage = "Deletion reason is limited to 500 characters")]
        //CORE_PORT: Not allowed
        //[AllowHtml]
        public string Reason {
            get {
                return _reason;
            }
            set {
                _reason = value.StripWhiteSpace();
            }
        }
    }
}
