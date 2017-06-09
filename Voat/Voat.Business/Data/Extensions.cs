using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voat.Common;
using Voat.Data;

namespace Voat.Data
{
    public static class Extensions
    {
        public static void Configure(this Microsoft.EntityFrameworkCore.DbContext context, DbContextOptionsBuilder optionsBuilder, string connectionStringName)
        {
            var conn = DataConfigurationSettings.Instance.Connections.FirstOrDefault(x => x.Name.IsEqual(connectionStringName));
            if (conn == null)
            {
                throw new ArgumentException($"Can not find connection with name '{connectionStringName}'", "connectionName");
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
        }
    }
}
