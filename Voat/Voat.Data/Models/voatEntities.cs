using System;
using Microsoft.EntityFrameworkCore;

namespace Voat.Data.Models
{
    //Stub out for ReadOnly db connections
    public partial class voatEntities : DbContext
    {

        public void EnableCacheableOutput()
        {
            //CORE_PORT: not supported
            /*
                this.Configuration.LazyLoadingEnabled = false;
                this.Configuration.ProxyCreationEnabled = false;
            */
        }

        //public voatEntities(bool readWrite)
        //    : this(readWrite ? "voatEntities" : "voatEntitiesReadOnly")
        //{
        //    /*no-op*/
        //}

        public voatEntities(string connectionName)
        //CORE_PORT: not supported
        //: base(String.Format("name={0}", connectionName))
        {
            /*no-op*/
        }

        //IAmAGate: Move queries to read-only mirror
        public voatEntities(bool useReadOnlyOnUnAthenticated)
            //CORE_PORT: not supported
            //:
            //this(useReadOnlyOnUnAthenticated && (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            //? "voatEntitiesReadOnly" : "voatEntities")
        {
            /*no-op*/
        }

    }
}