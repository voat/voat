using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Models;

namespace Voat.UI.Areas.Admin.Models
{
    public class BanViewModel
    {
        [Required]
        [StringLength(1000)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
        public SafeEnum<BanType> BanType { get; set; } = Domain.Models.BanType.Domain;
    }
}
