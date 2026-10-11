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
        static Extensions()
        {
            // Npgsql 6+ throws when a DateTime with Kind=Utc is written to a "timestamp
            // without time zone" column. Voat's schema stores plain "timestamp" columns
            // and all code writes DateTime.UtcNow (UTC values, matching the schema's
            // (now() at time zone 'utc') defaults). This switch restores the pre-6.0
            // behavior this codebase was built against: DateTime values are written
            // as-is (no conversion) and read back with Kind=Unspecified. It must be set
            // before the first type mapping is resolved, which is guaranteed here since
            // every DbContext (EF and Dapper paths alike) configures through Configure().
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

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
