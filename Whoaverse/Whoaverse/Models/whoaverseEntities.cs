using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Voat.Models
{
    //Stub out for ReadOnly db connections
    public partial class whoaverseEntities : DbContext
    {
        public whoaverseEntities(string connectionName) :base (String.Format("name={0}", connectionName)) { 
            /*no-op*/
        }
        //IAmAGate: Move queries to read-only mirror
        public whoaverseEntities(bool useReadOnlyOnUnAthenticated) : 
            this(useReadOnlyOnUnAthenticated && (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.User.Identity.IsAuthenticated) 
            ? "whoaverseEntitiesReadOnly" : "whoaverseEntities") { 
            /*no-op*/
        }
    }
}