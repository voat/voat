using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Data.Models
{
    //Stub out for ReadOnly db connections
    public partial class voatEntities : DbContext
    {

        public voatEntities(string connectionName)
            : base(String.Format("name={0}", connectionName))
        {
            /*no-op*/
        }
        

        //IAmAGate: Move queries to read-only mirror
        public voatEntities(bool useReadOnlyOnUnAthenticated) :
            this(useReadOnlyOnUnAthenticated && (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            ? "voatEntitiesReadOnly" : "voatEntities")
        {
            /*no-op*/
        }

        //public voatEntities(bool useReadOnlyOnUnAthenticated) :
        //    this(useReadOnlyOnUnAthenticated && (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
        //    ? CONSTANTS.CONNECTION_READONLY : CONSTANTS.CONNECTION_LIVE)
        //{
        //    /*no-op*/
        //}
    }
}
