using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public whoaverseEntities(DbConnection existingConnection, bool contextOwnsConnection) : base (existingConnection, contextOwnsConnection)
        {
            
        }
    }
}