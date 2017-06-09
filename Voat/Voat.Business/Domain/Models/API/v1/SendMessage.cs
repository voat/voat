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

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Voat.Domain.Models
{
    public class SendMessage
    {
        private string _sender = null;

        [Required]
        public string Message { get; set; }

        [Required]
        public string Recipient { get; set; }

        [JsonIgnore]
        public string Sender
        {
            get
            {
                //if (string.IsNullOrEmpty(_sender) && UserIdentity.IsAuthenticated)
                //{
                //    return UserIdentity.UserName;
                //}
                return _sender;
            }

            set
            {
                _sender = value;
            }
        }

        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }
    }
}
