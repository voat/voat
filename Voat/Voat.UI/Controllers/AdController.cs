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

                // get currently running ad for target subverse
                if (subverse != null)
                {
                    var subverseAd = db.Ads.FirstOrDefault(x => x.Subverse.Equals(subverse, StringComparison.InvariantCultureIgnoreCase) && x.EndDate > DateTime.Now);
                    if (subverseAd != null)
                    {
                        var adModel = new AdViewModel
                        {
                            Name = subverseAd.Name,
                            DestinationUrl = subverseAd.DestinationUrl,
                            Description = subverseAd.Description,
                            GraphicUrl = subverseAd.GraphicUrl
                        };

                        return View("_Ad", adModel);
                    }
                }

                // no running ad for target subverse found, get currently running global ad instead
                var globalAd = db.Ads.FirstOrDefault(x => x.EndDate > DateTime.Now && x.Subverse == null);
                if (globalAd != null)
                {
                    var adModel = new AdViewModel
                    {
                        Name = globalAd.Name,
                        DestinationUrl = globalAd.DestinationUrl,
                        Description = globalAd.Description,
                        GraphicUrl = globalAd.GraphicUrl
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