using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Voat.Models.ViewModels
{
    public class ApiKeyCreateRequest
    {

        [Display(Name = "App Name")]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "App Description")]
        [StringLength(2000)]
        public string Description { get; set; }

        [Display(Name = "App About Url")]
        [StringLength(200)]
        [Url(ErrorMessage = "Please enter a valid http, https, or ftp URL.")]
        public string AboutUrl { get; set; }


        [Display(Name = "OAuth Redirect Url")]
        [StringLength(500)]
        [Url(ErrorMessage = "Please enter a valid http, https, or ftp URL.")]
        public string RedirectUrl { get; set; }
        

    }
}