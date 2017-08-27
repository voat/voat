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
            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.PostgreSql:

                    //modelBuilder.Entity<Subverse>().Property(x => x.Name).HasColumnType("citext");
                    //modelBuilder.Entity<Subverse>().Property(x => x.Title).HasColumnType("citext");
                    modelBuilder.Entity<Subverse>().Property(x => x.Description).HasColumnType("citext");


                    //We have to use the modelBuilder here to set up the citext handling. This has to be programatic to support portability

                    //This is an EF6 way of telling the runtime to use citext for string columns. We need an EF Core way
                    //modelBuilder.Properties<string>().Configure(s => s.HasColumnType("public.citext"));

                    //var x  = modelBuilder.Model.Npgsql().GetOrAddPostgresExtension("public.citext");

                    //modelBuilder.Entity<Submission>(x => x.p

                    break;
            }

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
