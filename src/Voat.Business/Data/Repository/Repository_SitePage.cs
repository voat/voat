using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voat.Data.Models;
using Voat.Utilities;

namespace Voat.Data
{
    /// <summary>
    /// Storage for the owner-editable help pages. Name is the lowercase slug the
    /// /help/{pagetoshow} route uses ("useragreement", "privacy").
    /// </summary>
    public partial class Repository
    {
        /// <summary>
        /// Returns the owner's saved markdown, or null when no row exists — the
        /// null is the caller's signal to render SitePageDefaults instead.
        /// </summary>
        public async Task<SitePage> GetSitePage(string name)
        {
            return await _db.SitePage
                .Where(x => x.Name == name)
                .FirstOrDefaultAsync()
                .ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
        }

        /// <summary>
        /// Inserts or updates the page body. Updates in place so the row keeps a
        /// single author/date stamp rather than accumulating revisions.
        /// </summary>
        public async Task SaveSitePage(string name, string content, string userName)
        {
            var page = await _db.SitePage
                .Where(x => x.Name == name)
                .FirstOrDefaultAsync()
                .ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            if (page == null)
            {
                page = new SitePage { Name = name };
                _db.SitePage.Add(page);
            }

            page.Content = content;
            page.UpdatedBy = userName;
            page.LastUpdateDate = Repository.CurrentDate;

            await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
        }
    }
}
