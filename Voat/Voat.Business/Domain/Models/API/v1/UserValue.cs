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
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class UserValue
    {
        public UserValue()
        {
            /*no-op*/
        }

        public UserValue(string value)
        {
            this.Value = value;
        }

        [IgnoreDataMember]
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                return !String.IsNullOrEmpty(this.Value);
            }
        }

        /// <summary>
        /// Content of request
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonUnprintableCharConverter))]
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
