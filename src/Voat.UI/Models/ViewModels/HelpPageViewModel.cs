using System;
using System.ComponentModel.DataAnnotations;

namespace Voat.Models.ViewModels
{
    /// <summary>
    /// A site-owned help page (User Agreement, Privacy Policy) as the view needs
    /// it. Content stays raw markdown — the view renders it, matching how
    /// SubverseSettings handles its SideBar text.
    /// </summary>
    public class HelpPageViewModel
    {
        //Route slug, one of SitePageDefaults.GetDefault's known names.
        public string Name { get; set; }

        //Raw markdown: rendered in the body and pre-filled into the editor.
        public string Content { get; set; }

        //Only the owner sees the Edit affordance. Enforcement is server-side in
        //HomeController.UpdateHelpPage — this flag is purely display.
        public bool IsSiteOwner { get; set; }
    }

    /// <summary>
    /// POST body for saving an edited help page.
    /// </summary>
    public class HelpPageEditModel
    {
        public string Name { get; set; }

        [StringLength(30000)]
        [Required]
        public string Content { get; set; }
    }
}
