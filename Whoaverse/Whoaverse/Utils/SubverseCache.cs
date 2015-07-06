using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voat.Models;

namespace Voat.Utils
{
    public static class SubverseCache
    {

        public static Subverse GetSubverseInfo(string subverse) {

            if (!String.IsNullOrEmpty(subverse)) { 
                string cacheKey = String.Format("sub.{0}.info", subverse).ToLower();

                Subverse sub = (Subverse)CacheHandler.GetData(cacheKey);
                if (sub == null) {
                    sub = (Subverse)CacheHandler.Register(cacheKey, new Func<object>(() => {

                        using (whoaverseEntities db = new whoaverseEntities()) {
                            return db.Subverses.Where(x => x.name == subverse).FirstOrDefault();
                        }
                
                    }), TimeSpan.FromMinutes(5), true);
                }
                return sub;
            }
            return null;
        }

    }
}