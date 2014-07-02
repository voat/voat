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

using System.ComponentModel.DataAnnotations;

namespace Whoaverse.Models.ViewModels
{
    public class PrivateMessageComposeViewModel
    {
        [RegularExpression("^[a-zA-Z0-9-_]+$")]
        [Required(ErrorMessage = "Please enter a username to send this message to.")]
        [StringLength(20, ErrorMessage = "Username is limited to 20 characters.")]
        public string Recipient { get; set; }
        
        [Required(ErrorMessage = "Please enter a subject for this message.")]
        [StringLength(50, ErrorMessage = "The subject is limited to 50 characters.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter the body text for this message.")]
        [StringLength(4000, ErrorMessage = "Body text is limited to 4000 characters.")]
        public string Body { get; set; }
    }
}
