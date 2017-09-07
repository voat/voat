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
using System.Text;
using System.Threading.Tasks;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class Set
    {
        public string FullName { get { return new DomainReference(DomainType.Set, Name, UserName).FullName; } }

        public int ID { get; set; }

        [Required(ErrorMessage = "A set name is required")]
        [MaxLength(20, ErrorMessage = "Name is limited to 20 characters")]
        [RegularExpression(CONSTANTS.SUBVERSE_REGEX, ErrorMessage = "Set name can not contain anything but letters and numbers and is limited to 20 characters")]
        public string Name { get; set; }

        public string UserName { get; set; }

        [MaxLength(100, ErrorMessage = "Title is limited to 100 characters")]
        [Display(Name="Short Title")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Description is limited to 500 characters")]
        public string Description { get; set; }

        public SetType Type { get; set; }

        public int SubscriberCount { get; set; }

        public System.DateTime CreationDate { get; set; }

        [Display(Name = "Public")]
        public bool IsPublic { get; set; }
    }
}
