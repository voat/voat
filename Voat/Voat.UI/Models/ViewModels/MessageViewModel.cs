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

namespace Voat.Models.ViewModels
{

    public class MessageReplyViewModel
    {
        [Required(ErrorMessage = "A message ID must be provided to reply")]
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter some text")]
        [StringLength(10000, ErrorMessage = "Body is limited to 10000 characters")]
        [AllowHtml]
        public string Body { get; set; }
    }

    public class NewMessageViewModel
    {

        public string Sender { get; set; }

        //[RegularExpression("^[a-zA-Z0-9-_]+$", ErrorMessage="Please use only alphanumeric characters.")]
        [Required(ErrorMessage = "Please enter a username")]
        //[StringLength(23, ErrorMessage = "Username is limited to 23 characters.")]
        public string Recipient { get; set; }
        
        [Required(ErrorMessage = "Please enter a subject")]
        [StringLength(200, ErrorMessage = "The subject is limited to 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter some text")]
        [StringLength(10000, ErrorMessage = "Body is limited to 10000 characters")]
        [AllowHtml]
        public string Body { get; set; }

        public bool RequireCaptcha { get; set; }
    }
}
