using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class AdController : BaseController
    {
        // GET: Ad
        public async Task<ActionResult> RenderAd(string subverse)
        {
            if (Settings.AdsEnabled)
            {
                // TODO: check if currently logged in user is a donor or subscriber and has opted out of ads, if so, do not display any ads whatsoever
                var renderAd = true;

                ////Turn off ads for Donors
                //var q = new QueryUserData(User.Identity.Name);
                //var userData = await q.Execute();
                //if (userData != null && userData.Information.Badges.Any(x => x.Name.StartsWith("Donor")))
                //{
                //    renderAd = userData.Preferences.DisplayAds;
                //}

                if (renderAd)
                {
                    return View("_Ad", GetAdModel(subverse, true));
                }
                else
                {
                    return View("_Ad"); //a null model will prevent ad from rendering any html
                }
            }

            return View("_Ad"); //a null model will prevent ad from rendering any html
        }

        private AdViewModel GetAdModel(string subverse, bool useExisting = false)
        {
            var linkToAdPurchase = String.Format("[Want to advertize on {1}?]({0})", Url.Action("Advertize", "Home"), Settings.SiteName);

            //Default add
            var adToDisplay = new AdViewModel
            {
                Name = $"Advertize on {Settings.SiteName}",
                DestinationUrl = Url.Action("Advertize", "Home"),
                Description = linkToAdPurchase,
                GraphicUrl = Url.Content("~/Graphics/voat-ad-placeholder.png")
            };

            try
            {
                using (var db = new voatEntities())
                {
                    var ad = (from x in db.Ads
                              where
                              ((subverse != null && x.Subverse.Equals(subverse, StringComparison.InvariantCultureIgnoreCase) || (x.Subverse == null)))
                              && (x.EndDate >= Repository.CurrentDate && x.StartDate <= Repository.CurrentDate)
                              && x.IsActive
                              orderby x.Subverse descending
                              select x).FirstOrDefault();

                    if (ad != null)
                    {
                        adToDisplay = new AdViewModel
                        {
                            Name = ad.Name,
                            DestinationUrl = ad.DestinationUrl,
                            Description = String.Format("{0}\n\n\n\n{1}", ad.Description, linkToAdPurchase),
                            GraphicUrl = ad.GraphicUrl
                        };
                    }
                    else if (useExisting)
                    {
                        {
                            var ads = CacheHandler.Instance.Register(CachingKey.AdCache(), new Func<IList<Ad>>(() =>
                            {
                                using (var dbcontext = new voatEntities())
                                {
                                    var adCache = (from x in dbcontext.Ads
                                                   where
                                                   x.Subverse == null
                                                   && x.IsActive
                                                   select x).ToList();
                                    return adCache;
                                }
                            }), TimeSpan.FromMinutes(60));
                            if (ads != null && ads.Count > 0)
                            {
                                //pick random index
                                Random m = new Random();
                                var index = m.Next(0, ads.Count - 1);
                                ad = ads[index];

                                adToDisplay = new AdViewModel
                                {
                                    Name = ad.Name,
                                    DestinationUrl = ad.DestinationUrl,
                                    Description = String.Format("{0}\n\n\n\n{1}", ad.Description, linkToAdPurchase),
                                    GraphicUrl = ad.GraphicUrl
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                /*no-op - ensure that ads don't throw exceptions */
            }
            return adToDisplay;
        }

    public ViewResult RenderPlaceholder()
    {
        return View("_AdPlaceholder");
    }
}
}
