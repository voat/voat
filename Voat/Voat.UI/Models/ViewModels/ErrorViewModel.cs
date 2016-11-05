using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Models.ViewModels
{
    public class ErrorViewModel
    {
        public string Title { get; set; } = "Whoops!";
        public string Description { get; set; } = "Well this is embarrassing. Something went wrong and let&#39;s face it, nobody&#39;s happy about it.\nWe&#39;ll dispatch our monstersquad to take a look at it right away!";
        public string FooterMessage { get; set; } = "Thank you for being a chap.";
    }
}