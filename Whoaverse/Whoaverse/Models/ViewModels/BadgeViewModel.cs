namespace Voat.Models.ViewModels
{
    using System;

    public class BadgeViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Graphics { get; set; }
        public string Title { get; set; }
        public DateTime Awarded { get; set; }
    }
}