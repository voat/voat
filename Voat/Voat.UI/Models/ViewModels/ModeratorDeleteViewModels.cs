using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Utilities;

namespace Voat.Models.ViewModels
{
    public class ModeratorDeleteContentViewModel
    {
        private string _reason = "";
        [Required(ErrorMessage = "Expecting an ID")]

        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter a deletion reason")]
        [StringLength(500, ErrorMessage = "Deletion reason is limited to 500 characters")]
        [AllowHtml]
        public string Reason {
            get {
                return _reason;
            }
            set {
                _reason = Formatting.StripWhiteSpace(value);
            }
        }
    }
}