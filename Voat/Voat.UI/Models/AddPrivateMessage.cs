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
    public class AddPrivateMessage
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "PM author is required.")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Message text is required. Please fill this field.")]
        [StringLength(4000, ErrorMessage = "Message text is limited to 4.000 characters.")]
        public string PrivateMessageContent { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }
    }
}