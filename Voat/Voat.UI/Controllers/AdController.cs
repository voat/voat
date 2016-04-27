using System;
using System.Linq;
using System.Web.Mvc;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class AdController : Controller
    {
        // GET: Ad
        public ActionResult RenderAd(string subverse)
        {
            if (Settings.AdsEnabled)
            {
                using (var db = new voatEntities())
                {
                    // TODO: check if currently logged in user is a donor or subscriber and has opted out of ads, if so, do not display any ads whatsoever
                    var userData = new QueryUserData(User.Identity.Name);

                    var renderAd = true;

                    if (renderAd)
                    {
                        var ad = (from x in db.Ads
                                  where
                                  ((subverse != null && x.Subverse.Equals(subverse, StringComparison.InvariantCultureIgnoreCase) || (x.Subverse == null)))
                                  && (x.EndDate >= Repository.CurrentDate && x.StartDate <= Repository.CurrentDate)
                                  && x.IsActive
                                  orderby x.Subverse descending
                                  select x).FirstOrDefault();

                        var linkToAdPurchase = String.Format("[Want to advertize on Voat?]({0})", Url.Action("Advertize", "Home"));
                        if (ad != null)
                        {
                            var adModel = new AdViewModel
                            {
                                Name = ad.Name,
                                DestinationUrl = ad.DestinationUrl,
                                Description = String.Format("{0}\n\n{1}", ad.Description, linkToAdPurchase),
                                GraphicUrl = ad.GraphicUrl
                            };
                            return View("_Ad", adModel);
                        }

                        // no running ads found, render ad placeholder instead
                        var placeHolder = new AdViewModel
                        {
                            Name = "Advertize on Voat",
                            DestinationUrl = Url.Action("Advertize", "Home"),
                            Description = linkToAdPurchase,
                            GraphicUrl = Url.Content("~/Graphics/voat-ad-placeholder.png")
                        };
                        return View("_Ad", placeHolder);
                        //return RenderPlaceholder();
                    }
                    else
                    {
                        return View("_Ad"); //a null model will prevent ad from rendering any html
                    }
                }
            }
            return View("_Ad"); //a null model will prevent ad from rendering any html
        }

        public ViewResult RenderPlaceholder()
        {
            return View("_AdPlaceholder");
        }
    }
}