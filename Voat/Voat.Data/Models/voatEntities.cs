using System;
using Microsoft.EntityFrameworkCore;

namespace Voat.Data.Models
{
    //Stub out for ReadOnly db connections
    public partial class voatEntities : DbContext
    {

        public void EnableCacheableOutput()
        {
            // this.Configuration.LazyLoadingEnabled = false;//FIXME not supported
            // this.Configuration.ProxyCreationEnabled = false;
        }

        //public voatEntities(bool readWrite)
        //    : this(readWrite ? "voatEntities" : "voatEntitiesReadOnly")
        //{
        //    /*no-op*/
        //}

        public voatEntities(string connectionName)
        //: base(String.Format("name={0}", connectionName))//FIXME
        {
            /*no-op*/
        }

        //IAmAGate: Move queries to read-only mirror
        public voatEntities(bool useReadOnlyOnUnAthenticated) //:
                                                              //this(useReadOnlyOnUnAthenticated && (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.User.Identity.IsAuthenticated)//FIXME
                                                              //? "voatEntitiesReadOnly" : "voatEntities")
        {
            /*no-op*/
        }

    }
}