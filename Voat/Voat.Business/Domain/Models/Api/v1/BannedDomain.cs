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
