using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Voat.Data.Models;
using System.Linq;

namespace Voat.Data.Models
{
    public class VoatDataContext : VoatEntityContext
    {
        private string _connectionName;

        public VoatDataContext() : this("ReadWrite")
        {
        
        }
        public VoatDataContext(string connectionName)
        {
            _connectionName = connectionName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var conn = DataConfigurationSettings.Instance.Connections.FirstOrDefault(x => x.Name.IsEqual(_connectionName));
            if (conn == null)
            {
                throw new ArgumentException($"Can not find connection with name '{_connectionName}'", "connectionName");
            }

            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.SqlServer:
                    optionsBuilder.UseSqlServer(conn.Value);
                    break;
                case DataStoreType.PostgreSql:
                    optionsBuilder.UseNpgsql(conn.Value);
                    break;
            }

            base.OnConfiguring(optionsBuilder);
        }
        //CORE_PORT: Added this to ensure Dapper and direct connections can still execute
        public System.Data.Common.DbConnection Connection
        {
            get
            {
                return this.Database.GetDbConnection();
            }
        }
        public void EnableCacheableOutput()
        {
            //CORE_PORT: not supported
            /*
                this.Configuration.LazyLoadingEnabled = false;
                this.Configuration.ProxyCreationEnabled = false;
            */
        }

    }
}
