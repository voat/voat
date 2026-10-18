namespace Voat.Data.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The body of a site-level help page (User Agreement, Privacy Policy) as
    /// markdown, so the owner can edit it without touching source. Name is the
    /// lowercase slug the /help/{pagetoshow} route already uses.
    /// </summary>
    public partial class SitePage
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdateDate { get; set; }
    }
}
