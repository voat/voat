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

using System;
using System.ComponentModel.DataAnnotations;

namespace Voat.Models
{
    public class AddSet
    {
        public int SetId { get; set; }

        [StringLength(20, ErrorMessage = "The name length is limited to 20 characters.")]
        [Required(ErrorMessage = "Set name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Set description is required.")]
        public string Description { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Public { get; set; }

        public string Type { get; set; }
        public string Sidebar { get; set; }
        public Nullable<int> Subscribers { get; set; }

        [Required(ErrorMessage = "This setting is required.")]
        public bool Default { get; set; }
    }
}