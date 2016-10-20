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
using System.Collections.Generic;

namespace Voat.Domain.Models
{
    public class SubverseInformation
    {
        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        //public Nullable<bool> RatedAdult { get; set; }

        public string Sidebar { get; set; }

        public string FormattedSidebar { get; set; }

        public int SubscriberCount { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public bool? IsAnonymized { get; set; }

        public bool IsAdult { get; set; }

        //[JsonIgnore]
        //public bool IsAdminDisabled { get; set; }
        public IEnumerable<string> Moderators { get; set; }
    }
}
