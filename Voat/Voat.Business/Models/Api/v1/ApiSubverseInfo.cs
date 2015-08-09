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

using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
namespace Voat.Models.Api.v1
{


    public class ApiSubverseInfo
    {

        [JsonProperty("name")]
        [DataMember(Name = "name")]
        public string Name { get; set; }


        [JsonProperty("title")]
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        [DataMember(Name = "description")]
        public string Description { get; set; }


        [JsonProperty("creationDate")]
        [DataMember(Name = "creationDate")]
        public DateTime CreationDate { get; set; }


        [JsonProperty("subscriberCount")]
        [DataMember(Name = "subscriberCount")]
        public int SubscriberCount { get; set; }


        [JsonProperty("ratedAdult")]
        [DataMember(Name = "ratedAdult")]
        public Nullable<bool> RatedAdult { get; set; }

        [JsonProperty("sidebar")]
        [DataMember(Name = "sidebar")]
        public string Sidebar { get; set; }


        [JsonProperty("type")]
        [DataMember(Name = "type")]
        public string Type { get; set; }

    }
}