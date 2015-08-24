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
    public class AddSubmissionLinkpost
    {
        public int ID { get; set; }
        public Nullable<short> Votes { get; set; }
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public int Type { get; set; }

        public Nullable<double> Rank { get; set; }

        [Required(ErrorMessage = "Post title is required. Please fill this field.")]
        [StringLength(200, ErrorMessage = "The title must be at least 10 and no more than 200 characters long.", MinimumLength = 10)]
        public string LinkDescription { get; set; }

        [Required(ErrorMessage = "URL is required. Please fill this field.")]
        [Url(ErrorMessage="Please enter a valid http, https, or ftp URL.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "You must enter a subverse to send the post to. Examples: programming, videos, pics, funny...")]
        public string Subverse { get; set; }
    }
}