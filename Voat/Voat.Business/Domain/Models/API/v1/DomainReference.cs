using System;

namespace Voat.Domain.Models
{
    public class DomainReference
    {
        public DomainReference() { }

        public DomainReference(DomainType type, string name, string ownerName = null)
        {
            this.Type = type;
            this.Name = name;
            this.OwnerName = ownerName;
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
