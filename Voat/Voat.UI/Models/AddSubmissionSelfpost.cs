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
using System.Web.Mvc;

namespace Voat.Models
{
    public class AddSubmissionSelfpost
    {
        public int ID { get; set; }
        public Nullable<short> Votes { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        [Required]
        public int Type { get; set; }
        public Nullable<double> Rank { get; set; }

        [Required(ErrorMessage = "Message title is required. Please fill this field.")]
        [StringLength(200, ErrorMessage = "Submission title must be at least 10 and no more than 200 characters long.", MinimumLength = 10)]
        public string Title { get; set; }

        [StringLength(200, ErrorMessage = "Subverse title is limited to 20 characters.")]
        public string Subverse { get; set; }

        [AllowHtml]
        [StringLength(10000, ErrorMessage = "Submission text is limited to 10.000 characters.")]
        public string Content { get; set; }
    }
}
