using System;
using Microsoft.EntityFrameworkCore;
using Voat.Common;
using Voat.Data.Models;
using Voat.Utilities;

namespace Voat.Data
{
    /// <summary>
    /// Creates the SitePage table on databases provisioned before it existed.
    /// Fresh installs get it from misc/SqlScripts/*/voat.sql, but that script only
    /// runs once at volume creation and setup.sql needs psql, so this runs at
    /// startup instead and lands the DDL in the persistent data volume.
    ///
    /// The whole definition sits behind a single existence guard, so this is
    /// idempotent and will not touch a table voat.sql already created.
    /// </summary>
    public static class SitePageSchema
    {
        //Mirrors the "SitePage" definition in misc/SqlScripts/PostgreSql/voat.sql.
        private const string CREATE_POSTGRESQL = @"DO $$
BEGIN
  IF to_regclass('""SitePage""') IS NULL THEN
    CREATE TABLE ""SitePage""(
      ""ID"" int NOT NULL,
      ""Name"" varchar(50) NOT NULL,
      ""Content"" varchar,
      ""UpdatedBy"" varchar(50),
      ""LastUpdateDate"" timestamp);
    CREATE SEQUENCE ""sitepage_id_seq"" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY ""SitePage"".""ID"";
    ALTER TABLE ""SitePage"" ALTER COLUMN ""ID"" SET DEFAULT nextval('""sitepage_id_seq""');
    ALTER TABLE ""SitePage"" ALTER COLUMN ""LastUpdateDate"" SET DEFAULT (now() at time zone 'utc');
    ALTER TABLE ""SitePage"" ADD CONSTRAINT ""PK_SitePage"" PRIMARY KEY (""ID"");
    ALTER TABLE ""SitePage"" ADD CONSTRAINT ""IX_SitePage_Name"" UNIQUE (""Name"");
  END IF;
END
$$;";

        //Mirrors the [dbo].[SitePage] definition in misc/SqlScripts/SqlServer/voat.sql.
        private const string CREATE_SQLSERVER = @"IF OBJECT_ID(N'[dbo].[SitePage]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SitePage](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        [Content] [nvarchar](max) NULL,
        [UpdatedBy] [nvarchar](50) NULL,
        [LastUpdateDate] [datetime] NULL,
     CONSTRAINT [PK_SitePage] PRIMARY KEY CLUSTERED
    (
        [ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
     CONSTRAINT [IX_SitePage_Name] UNIQUE NONCLUSTERED
    (
        [Name] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[SitePage] ADD CONSTRAINT [DF_SitePage_LastUpdateDate] DEFAULT (getutcdate()) FOR [LastUpdateDate]
END";

        /// <summary>
        /// Ensures the SitePage table exists. Safe to call on every startup.
        /// Throws when the DDL cannot be applied: the help pages cannot function
        /// without this table, so failing here is louder than failing per request.
        /// </summary>
        public static void EnsureSitePageTable()
        {
            string sql;
            switch (DataConfigurationSettings.Instance.StoreType)
            {
                case DataStoreType.PostgreSql:
                    sql = CREATE_POSTGRESQL;
                    break;
                case DataStoreType.SqlServer:
                    sql = CREATE_SQLSERVER;
                    break;
                default:
                    throw new NotImplementedException();
            }

            try
            {
                using (var db = new VoatDataContext(CONSTANTS.CONNECTION_LIVE))
                {
                    db.Database.ExecuteSqlRaw(sql);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Could not create the SitePage table. The User Agreement and Privacy Policy pages need it. " +
                    "Check that the configured database account may run DDL, or apply the SitePage section of " +
                    "misc/SqlScripts/*/voat.sql manually.", ex);
            }
        }
    }
}
