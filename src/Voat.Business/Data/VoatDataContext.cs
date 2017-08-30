using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Voat.Data.Models;
using System.Linq;
using Voat.Common;
using Voat.Utilities;

namespace Voat.Data.Models
{
    //All UI based access of EF context should go through this object.
    //At a later date will will throw errors in this object to force no usage from the UI project 
    //TODO: Implement Command/Query - Remove direct DataContext access from UI project
    [Obsolete("Move any logic to Repository")]
    public class VoatOutOfRepositoryDataContextAccessor : VoatDataContext
    {
        public VoatOutOfRepositoryDataContextAccessor(NotImplementedException exception) : base("fake") { }

        public VoatOutOfRepositoryDataContextAccessor(string name = CONSTANTS.CONNECTION_LIVE) : base(name) { }
    }

    //should ONLY be accessed in Repository class
    public class VoatDataContext : VoatEntityContext
    {
        private string _connectionName;

        public VoatDataContext() : this(CONSTANTS.CONNECTION_LIVE)
        {
        
        }
        public VoatDataContext(string connectionName)
        {
            _connectionName = connectionName;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            this.Configure(optionsBuilder, _connectionName);
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SqlFormatter.DefaultSchema);
            base.OnModelCreating(modelBuilder);
        }
        //Added this to ensure Dapper and direct connections can still execute
        public System.Data.Common.DbConnection Connection
        {
            get
            {
                return this.Database.GetDbConnection();
            }
        }
    }
}
