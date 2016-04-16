using System;
using System.Linq;
using System.Web.Mvc;
using Voat.Data.Models;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class AdController : Controller
    {
        // GET: Ad
        public ActionResult RenderAd(string subverse)
        {
            using (var db = new voatEntities())
            {
                // TODO: check if currently logged in user is a donor or subscriber and has opted out of ads, if so, do not display any ads whatsoever

                var ad = (from x in db.Ads
                          where
                          ((subverse != null && x.Subverse.Equals(subverse, StringComparison.InvariantCultureIgnoreCase) || (subverse == null && x.Subverse == null)))
                          && (x.EndDate >= DateTime.Now && x.StartDate <= DateTime.Now)
                          orderby x.Subverse descending
                          select x).FirstOrDefault();

                if (ad != null)
                {
                    var adModel = new AdViewModel
                    {
                        Name = ad.Name,
                        DestinationUrl = ad.DestinationUrl,
                        Description = ad.Description,
                        GraphicUrl = ad.GraphicUrl
                    };
                    return View("_Ad", adModel);
                }
            }
            // no running ads found, render ad placeholder instead
            return RenderPlaceholder();
        }

        public ViewResult RenderPlaceholder()
        {
            return View("_AdPlaceholder");
        }
    }
}