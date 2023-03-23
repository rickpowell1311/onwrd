using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators
{
    internal class PostgreSqlDatabaseMigrator : IDatabaseMigrator
    {
        public async Task MigrateAsync(DbConnection connection)
        {
            await connection.ExecuteSqlAsync(CreateSchemaSql());
            await connection.ExecuteSqlAsync(CreateEventsTableSql());
        }

        public void Migrate(DbConnection connection)
        {
            connection.ExecuteSql(CreateSchemaSql());
            connection.ExecuteSql(CreateEventsTableSql());
        }

        private static string CreateSchemaSql()
        {
            return "CREATE SCHEMA IF NOT EXISTS \"Onwrd\";";
        }

        private static string CreateEventsTableSql()
        {
            return @"
                CREATE TABLE IF NOT EXISTS ""Onwrd"".""Events""
                (
	                ""Id"" UUID NOT NULL,
	                ""CreatedOn"" timestamp(6) NOT NULL,
	                ""DispatchedOn"" timestamp(6) NULL,
	                ""TypeId"" TEXT NOT NULL,
	                ""Contents"" TEXT NOT NULL,
	                ""AssemblyName"" TEXT NOT NULL,
	                CONSTRAINT ""PK_Onwrd_Events"" PRIMARY KEY (""Id"")
                );

                CREATE INDEX IF NOT EXISTS ""IX_Onwrd_Events_DispatchedOn"" ON ""Onwrd"".""Events"" USING btree (""DispatchedOn"");
                END";
        }
    }
}
