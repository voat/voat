using System;

namespace Voat.Domain.Models
{
    public class DomainReference
    {
        public DomainReference() { }

        private static readonly string seperator = "/";

        public DomainReference(DomainType type, string name, string ownerName = null)
        {
            this.Type = type;
            this.Name = name;
            this.OwnerName = ownerName;
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
                    return Name + seperator + OwnerName;
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

        public static DomainReference ParseSetFromFullName(string fullName)
        {
            DomainReference d = null;
            if (!String.IsNullOrEmpty(fullName))
            {
                d = new DomainReference();
                d.Type = DomainType.Set;
                if (fullName.Contains(seperator))
                {
                    var split = fullName.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
                    d.Name = split[0];
                    d.OwnerName = split[1];
                }
                else
                {
                    d.Name = fullName;
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
