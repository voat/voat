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
    //should ONLY be access in Repository class
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
                //System.Data.Common.DbConnection conn = null;
                //var connString = DataConfigurationSettings.Instance.Connections.FirstOrDefault(x => x.Name.IsEqual("ReadWrite"));
                //if (connString == null)
                //{
                //    throw new ArgumentException($"Can not find connection with name 'ReadWrite'", "connectionName");
                //}
                ////HACK FOR PG Not being updated for Core 2 Preview 2
                //switch (DataConfigurationSettings.Instance.StoreType)
                //{
                //    case DataStoreType.SqlServer:
                //        conn = new System.Data.SqlClient.SqlConnection(connString.Value);
                //        break;
                //    case DataStoreType.PostgreSql:
                //        conn = new Npgsql.NpgsqlConnection(connString.Value);
                //        break;
                //}

                //return conn;

                return this.Database.GetDbConnection();
            }
        }
    }
}
