using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Voat.Domain.Models.Input
{
    public class SubverseFlairInput
    {
        [Required]
        public string Subverse { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Flair label must be between {2} and {1} characters", MinimumLength = 1)]
        public string Label { get; set; }

        [StringLength(50, ErrorMessage = "Flair css class must be between {2} and {1} characters", MinimumLength = 1)]
        public string CssClass { get; set; }
    }
}
