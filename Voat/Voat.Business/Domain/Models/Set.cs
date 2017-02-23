using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class Set
    {

        public int ID { get; set; }

        [Required(ErrorMessage = "A set name is required")]
        [MaxLength(20, ErrorMessage = "Name is limited to 20 characters")]
        [RegularExpression(CONSTANTS.SUBVERSE_REGEX, ErrorMessage = "Set name can not contain anything but letters and numbers")]
        public string Name { get; set; }

        public string UserName { get; set; }

        [MaxLength(100, ErrorMessage = "Title is limited to 100 characters")]
        [Display(Name="Short Title")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Description is limited to 500 characters")]
        public string Description { get; set; }

        public SetType Type { get; set; }

        public int SubscriberCount { get; set; }

        public System.DateTime CreationDate { get; set; }

        public bool IsPublic { get; set; }
    }
}
