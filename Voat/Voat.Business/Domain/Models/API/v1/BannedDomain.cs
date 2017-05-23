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

using System;

namespace Voat.Domain.Models
{
    /// <summary>
    /// Represents a banned entity
    /// </summary>
    public class BannedItem
    {
        /// <summary>
        /// The date the ban was put in place
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The name of the banned item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The reason given for the ban
        /// </summary>
        public string Reason { get; set; }
    }
}
