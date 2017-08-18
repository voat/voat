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
using System.Text.RegularExpressions;
using Voat.Common;

namespace Voat.Domain.Models
{
    public class ContentReference
    {
        public int ID { get; set; }

        public ContentType Type { get; set; }

    }
    public class DomainReference
    {
        public DomainReference() { }

        public DomainReference(DomainType type, string name, string ownerName = null)
        {
            this.Type = type;
            this.Name = name;
            this.OwnerName = Utilities.CONSTANTS.SYSTEM_USER_NAME.IsEqual(ownerName) ? null : ownerName;
        }

        /// <summary>
        /// Returns the fully qualified name of the domain reference
        /// </summary>
        public string FullName
        {
            get
            {
                if (Type == DomainType.Set && !String.IsNullOrEmpty(OwnerName))
                {
                    return Name + Utilities.CONSTANTS.SET_SEPERATOR + OwnerName;
                }
                return Name;
            }
        }

        /// <summary>
        /// Specifies the name of the domain object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the type of domain object.
        /// </summary>
        public DomainType Type { get; set; }

        /// <summary>
        /// Specifies the owner name of the domain object. Used with Sets.
        /// </summary>
        public string OwnerName { get; set; }

        public static DomainReference Parse(string fullName, DomainType domainType)
        {
            DomainReference d = null;
            Match match = null;

            if (!String.IsNullOrEmpty(fullName))
            {
                switch (domainType)
                {
                    case DomainType.Set:

                        match = Regex.Match(fullName, String.Format("^{0}$", Utilities.CONSTANTS.SET_REGEX));

                        if (match.Success)
                        {
                            d = new DomainReference();
                            d.Type = domainType;
                            d.Name = match.Groups["name"].Value;
                            d.OwnerName = match.Groups["ownerName"].Success ? match.Groups["ownerName"].Value : null;
                        }

                        break;
                    case DomainType.Subverse:
                        match = Regex.Match(fullName, String.Format("^{0}$", Utilities.CONSTANTS.SUBVERSE_REGEX));

                        if (match.Success)
                        {
                            d = new DomainReference();
                            d.Type = domainType;
                            d.Name = match.Value;
                            d.OwnerName = null;
                        }

                        break;
                    default:

                        break;
                }
            }
            return d;
        }
    }

    public class BlockedItem : DomainReference
    {
        public DateTime? CreationDate { get; set; }
    }

    public class DomainReference<T> : DomainReference
    {
        /// <summary>
        /// Specifies additional data.
        /// </summary>
        public T Data { get; set; }
    }
}
