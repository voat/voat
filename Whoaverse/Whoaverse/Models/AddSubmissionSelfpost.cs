/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Whoaverse.Models
{
    public class AddSubmissionSelfpost
    {
        public int Id { get; set; }
        public Nullable<short> Votes { get; set; }
        public string Name { get; set; }
        public System.DateTime Date { get; set; }
        public int Type { get; set; }
        public Nullable<double> Rank { get; set; }


        [Required(ErrorMessage = "Message title is required. Please fill this field.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "You must choose a subverse to send the post to. Example: programming, videos, pics, funny.")]
        public string Subverse { get; set; }

        [StringLength(10000, ErrorMessage = "Submission text is limited to 10.000 characters.")]
        public string MessageContent { get; set; }
    }
}