using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voat.Models;

namespace Voat.Utils
{
    public static class SubverseCache
    {
        public static void Remove(string subverse) {
            CacheHandler.Remove(CacheHandler.Keys.SubverseInfo(subverse));
        }
        public static Subverse Retrieve(string subverse) {

            if (!String.IsNullOrEmpty(subverse)) { 
                string cacheKey = CacheHandler.Keys.SubverseInfo(subverse);

                Subverse sub = (Subverse)CacheHandler.Retrieve(cacheKey);
                if (sub == null) {
                    sub = (Subverse)CacheHandler.Register(cacheKey, new Func<object>(() => {

                        using (whoaverseEntities db = new whoaverseEntities()) {
                            return db.Subverses.Where(x => x.name == subverse).FirstOrDefault();
                        }
                
                    }), TimeSpan.FromMinutes(5), 50);
                }
                return sub;
            }
            return null;
        }

    }
}